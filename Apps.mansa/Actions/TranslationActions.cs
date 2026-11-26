using Apps.Mansa.Models;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
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

        return new TextResponse
        {
            TranslatedText = data.Translation ?? data.Text,
            Characters = input.Text?.Length ?? 0,
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

            async Task<IEnumerable<TextResponse>> BatchTranslate(IEnumerable<(Unit Unit, Segment Segment)> batch)
            {
                var tasks = batch.Select(async x =>
                    await TranslateText(new TextTranslationRequest
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
            double cost = 0;
            foreach (var (unit, results) in translations)
            {
                foreach (var (segment, result) in results)
                {
                    segment.SetTarget(result.TranslatedText);
                    segment.State = SegmentState.Translated;
                    characters += result.Characters;
                    inputTokens += result.InputTokens;
                    outputTokens += result.OutputTokens;
                    cost += result.Cost;
                }
                unit.Provenance.Translation.Tool = "Mansa";
                unit.Provenance.Translation.ToolReference = "https://mymansa.ai";
            }

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

                return new FileResponse
                {
                    File = await _fileManagementClient.UploadAsync(originalStream, targetContent.OriginalMediaType, targetContent.OriginalName),
                    Characters = characters,
                    InputTokens = inputTokens,
                    OutputTokens = outputTokens,
                    Cost = cost
                };
            }
            else if (input.OutputFileHandling == "xliff1")
            {
                var xliff1String = Xliff1Serializer.Serialize(content);
                return new FileResponse
                {
                    File = await _fileManagementClient.UploadAsync(xliff1String.ToStream(), MediaTypes.Xliff, content.XliffFileName),
                    Characters = characters,
                    InputTokens = inputTokens,
                    OutputTokens = outputTokens,
                    Cost = cost
                };
            }

            return new FileResponse
            {
                File = await _fileManagementClient.UploadAsync(content.Serialize().ToStream(), MediaTypes.Xliff, content.XliffFileName),
                Characters = characters,
                InputTokens = inputTokens,
                OutputTokens = outputTokens,
                Cost = cost
            };
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
}
