using Apps.Mansa.Models;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Filters.Constants;
using Blackbird.Filters.Enums;
using Blackbird.Filters.Extensions;
using Blackbird.Filters.Transformations;
using Blackbird.Filters.Xliff.Xliff1;
using RestSharp;

namespace Apps.Mansa.Actions;

[ActionList("Translation")]
public class TranslationActions : MansaInvocable
{
    private readonly IFileManagementClient _fileManagementClient;

    public TranslationActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
        : this(invocationContext, fileManagementClient, null)
    {
    }

    internal TranslationActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient, string? baseUrl)
        : base(invocationContext, baseUrl)
    {
        _fileManagementClient = fileManagementClient;
    }

    [BlueprintActionDefinition(BlueprintAction.TranslateText)]
    [Action("Translate text", Description = "Translate a single simple text string")]
    public async Task<TextResponse> TranslateText([ActionParameter] TextTranslationRequest input)
    {
        var (data, sourceCharacters) = await CallTranslateApiAsync(input);

        if (!HasTranslationText(data, input.Text))
        {
            throw new PluginApplicationException("Mansa API returned no translation.");
        }

        return new TextResponse
        {
            TranslatedText = data.Translation!,
            Characters = sourceCharacters,
            InputTokens = data.InputTokens,
            OutputTokens = data.OutputTokens,
            IsValid = data.IsValid,
            NeedsReview = data.NeedsReview,
            Cost = data.Billing?.Cost ?? 0,
            CostUsd = data.Billing?.CostUsd,
            CreditsCharged = data.Billing?.CreditsCharged,
            BalanceBefore = data.Billing?.BalanceBefore ?? 0,
            BalanceAfter = data.Billing?.BalanceAfter ?? 0,
            Rate = data.Billing?.Pricing?.Rate
        };
    }

    [BlueprintActionDefinition(BlueprintAction.TranslateFile)]
    [Action("Translate", Description = "Translate file content retrieved from a CMS or file storage. The output can be used in compatible actions.")]
    public async Task<FileResponse> TranslateContent([ActionParameter] ContentTranslationRequest input)
    {
        if (string.IsNullOrWhiteSpace(input.TargetLanguage))
        {
            throw new PluginMisconfigurationException("The target language can not be empty, please fill the 'Target language' field");
        }

        try
        {
            var stream = await _fileManagementClient.DownloadAsync(input.File);
            var content = await Transformation.Parse(stream, input.File.Name);

            async Task<IEnumerable<(TranslateDataDto Data, int SourceCharacters)>> BatchTranslate(IEnumerable<(Unit Unit, Segment Segment)> batch)
            {
                var tasks = batch.Select(async x =>
                    await CallTranslateApiAsync(new TextTranslationRequest
                    {
                        Text = x.Segment.GetSource(),
                        From = input.SourceLanguage,
                        TargetLanguage = input.TargetLanguage,
                        Context = input.Context,
                        Creativity = input.Creativity
                    })
                );

                return await Task.WhenAll(tasks);
            }

            var translations = await content.GetUnits().Batch(100, x => !x.IsIgnorbale && x.IsInitial).Process(BatchTranslate);

            double characters = 0;
            double inputTokens = 0;
            double outputTokens = 0;
            int invalidSegments = 0;
            int needsReviewSegments = 0;
            double cost = 0;
            foreach (var (unit, results) in translations)
            {
                var translatedAnyInUnit = false;
                foreach (var (segment, result) in results)
                {
                    var data = result.Data;
                    var sourceText = segment.GetSource();
                    if (HasTranslationText(data, sourceText) && data.IsValid)
                    {
                        segment.SetTarget(data.Translation!);
                        segment.State = SegmentState.Translated;
                        translatedAnyInUnit = true;
                    }
                    else
                    {
                        invalidSegments++;
                    }

                    if (data.NeedsReview)
                    {
                        needsReviewSegments++;
                    }

                    characters += result.SourceCharacters;
                    inputTokens += data.InputTokens;
                    outputTokens += data.OutputTokens;
                    cost += data.Billing?.Cost ?? 0;
                }

                if (translatedAnyInUnit)
                {
                    unit.Provenance.Translation.Tool = "Mansa";
                    unit.Provenance.Translation.ToolReference = "https://mymansa.ai";
                }
            }

            FileResponse CreateFileResponse(FileReference uploadedFile) => new()
            {
                File = uploadedFile,
                Characters = characters,
                InputTokens = inputTokens,
                OutputTokens = outputTokens,
                InvalidSegments = invalidSegments,
                NeedsReviewSegments = needsReviewSegments,
                Cost = cost
            };

            if (input.OutputFileHandling == "original")
            {
                var targetContent = content.Target();
                Stream originalStream;
                try
                {
                    originalStream = targetContent.Serialize().ToStream();
                }
                catch (Exception e) when (e.Message.Contains("Cannot convert to content, no original data found"))
                {
                    throw new PluginMisconfigurationException("The original file content could not be retrieved because it is not supported for this file format. Please change the 'Output file handling' field to 'Interoperable XLIFF (default)'.");
                }

                return CreateFileResponse(
                    await _fileManagementClient.UploadAsync(originalStream, targetContent.OriginalMediaType, targetContent.OriginalName));
            }
            else if (input.OutputFileHandling == "xliff1")
            {
                var xliff1String = Xliff1Serializer.Serialize(content);
                return CreateFileResponse(
                    await _fileManagementClient.UploadAsync(xliff1String.ToStream(), MediaTypes.Xliff, content.XliffFileName));
            }

            return CreateFileResponse(
                await _fileManagementClient.UploadAsync(content.Serialize().ToStream(), MediaTypes.Xliff, content.XliffFileName));
        }
        catch (Exception e)
        {
            if (e.Message.Contains("This file format is not supported"))
            {
                throw new PluginMisconfigurationException("The file format is not supported by the Blackbird interoperable setting.");
            }
            throw;
        }
    }

    private async Task<(TranslateDataDto Data, int SourceCharacters)> CallTranslateApiAsync(TextTranslationRequest input)
    {
        var request = new RestRequest("v1/translate", Method.Post);

        var inputs = new Dictionary<string, object>
        {
            { "text", input.Text },
            { "to", input.TargetLanguage }
        };

        if (!string.IsNullOrWhiteSpace(input.From))
        {
            inputs.Add("from", input.From);
        }

        if (input.MaxTokens != null)
        {
            inputs.Add("maxTokens", input.MaxTokens);
        }

        if (input.Creativity != null)
        {
            inputs.Add("creativity", input.Creativity);
        }

        if (input.Context != null)
        {
            inputs.Add("context", input.Context);
        }

        var response = await Client.ExecuteJsonAsync<TranslateResponseDto>(request, inputs);
        var data = response?.Data
            ?? throw new PluginApplicationException("Mansa API response did not include a 'data' payload.");

        return (data, input.Text?.Length ?? 0);
    }

    private static bool HasTranslationText(TranslateDataDto data, string? sourceText) =>
        data.Translation is not null &&
        (!string.IsNullOrEmpty(data.Translation) || string.IsNullOrWhiteSpace(sourceText));
}
