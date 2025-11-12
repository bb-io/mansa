using Apps.Mansa.Models;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Blueprints;
using RestSharp;

namespace Apps.Mansa.Actions;

[ActionList("Translation")]
public class TranslationActions(InvocationContext invocationContext) : MansaInvocable(invocationContext)
{
    [BlueprintActionDefinition(BlueprintAction.TranslateText)]
    [Action("Translate text", Description = "Translate a single simple text string")]
    public async Task<TranslationResponse> TranslateText([ActionParameter] TextTranslationRequest input)
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

        return new TranslationResponse
        {
            TranslatedText = response.Translation,
            Characters = response.Billing.Characters,
            Cost = response.Billing.Cost,
            BalanceBefore = response.Billing.BalanceBefore,
            BalanceAfter = response.Billing.BalanceAfter,
            Rate = response.Billing.Pricing.Rate
        };
    }
}
