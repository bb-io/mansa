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
public class TranslationActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : MansaInvocable(invocationContext)
{
    [BlueprintActionDefinition(BlueprintAction.TranslateText)]
    [Action("Translate text", Description = "Translate a single simple text string")]
    public async Task<TextResponse> TranslateText([ActionParameter] TextTranslationRequest input)
    {
        var request = new RestRequest("translate", Method.Post);

        request.AddHeader("Content-Type", "application/json");

        var inputs = new Dictionary<string, object> 
        {
            {"text", input.Text },
            { "to", input.TargetLanguage},
            {"from" , input.From }
        };

        if (input.MaxTokens != null)
        {
            inputs.Add("max_tokens", input.MaxTokens);
        }

        if (input.Creativity != null)
        {
            inputs.Add("creativity", input.Creativity);
        }

        if (input.Context != null)
        {
            inputs.Add("context", input.Context);
        }
        
        var response = await Client.ExecuteWithTokenAsync<TranslateResponseDto>(request, inputs);

        return new TextResponse
        {
            TranslatedText = response.Translation,
            Characters = response.Billing.Characters,
            Cost = response.Billing.Cost,
            BalanceBefore = response.Billing.BalanceBefore,
            BalanceAfter = response.Billing.BalanceAfter,
            Rate = response.Billing.Pricing.Rate
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
            var stream = await fileManagementClient.DownloadAsync(input.File);
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

            var sourceLanguages = new List<string>();
            double Characters = 0;
            double Cost = 0;
            foreach (var (unit, results) in translations)
            {
                foreach (var (segment, result) in results)
                {
                    segment.SetTarget(result.TranslatedText);
                    segment.State = SegmentState.Translated;
                    Characters += result.Characters;
                    Cost += result.Cost;
                }
                unit.Provenance.Translation.Tool = "Mansa";
                unit.Provenance.Translation.ToolReference = "https://all-lab-portal.com/";
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
                    throw new PluginMisconfigurationException("The original file content could not be retrieved because it's supported only for html files. Please change the 'Output file handling' field to 'Interoperable XLIFF (default)' or use DeepL native file translation strategy.");
                }

                return new FileResponse
                {
                    File = await fileManagementClient.UploadAsync(originalStream, targetContent.OriginalMediaType, targetContent.OriginalName),
                    Characters = Characters,
                    Cost = Cost
                };
            }
            else if (input.OutputFileHandling == "xliff1")
            {
                var xliff1String = Xliff1Serializer.Serialize(content);
                return new FileResponse
                {
                    File = await fileManagementClient.UploadAsync(xliff1String.ToStream(), MediaTypes.Xliff, content.XliffFileName),
                    Characters = Characters,
                    Cost = Cost
                };
            }            

            return new FileResponse
            {
                File = await fileManagementClient.UploadAsync(content.Serialize().ToStream(), MediaTypes.Xliff, content.XliffFileName),
                Characters = Characters,
                Cost = Cost
            };
        
        }
        catch (Exception e)
        {
            if (e.Message.Contains("This file format is not supported"))
            {
                throw new PluginMisconfigurationException("The file format is not supported by the Blackbird interoperable setting. Try setting the file translation strategy to DeepL native.");
            }
            throw;
        }
    }

}
