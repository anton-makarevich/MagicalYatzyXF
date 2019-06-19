using System.Collections.Generic;
using System.Threading.Tasks;
using Sanet.MagicalYatzy.Common.Connected_Services.LegacyScoreService;

namespace Sanet.MagicalYatzy.Common.Extensions
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    public static class WcfServiceAsyncExtensions
    {
        /// <summary>
        /// Task wrapper for GetLastWeekChampionAsync() method
        /// </summary>
        public static Task<IEnumerable<string>> GetLastWeekChampionTaskAsync(this KniffelServiceSoapClient client, string rules)
        {
            var tcs = new TaskCompletionSource<IEnumerable<string>>();
            client.GetLastWeekChempionCompleted+= (s, e) =>
            {
                if (e.Error != null) tcs.SetException(e.Error);
                else if (e.Cancelled) tcs.SetCanceled();
                else tcs.SetResult(new List<string>() { e.Result.Body.Name, e.Result.Body.Score });
            };
            client.GetLastWeekChempionAsync(
                new GetLastWeekChempionRequest(new GetLastWeekChempionRequestBody (rules, "", "")));
            return tcs.Task;
        }

        /// <summary>
        /// Task wrapper for GetLastWeekChempionAsync() method
        /// </summary>
        public static Task<IEnumerable<string>> GetLastDayChampionTaskAsync(this KniffelServiceSoapClient client, string rules)
        {
            var tcs = new TaskCompletionSource<IEnumerable<string>>();
            client.GetLastDayChempionCompleted += (s, e) =>
            {
                if (e.Error != null) tcs.SetException(e.Error);
                else if (e.Cancelled) tcs.SetCanceled();
                else tcs.SetResult(new List<string>() { e.Result.Body.Name, e.Result.Body.Score });
            };
            client.GetLastDayChempionAsync( new GetLastDayChempionRequest( new GetLastDayChempionRequestBody(rules, "", "")));
            return tcs.Task;
        }

        /// <summary>
        /// Task wrapper for GetLastWeekChempionAsync() method
        /// </summary>
        public static Task<GetTopPlayersResponse> GetTopPlayersTaskAsync(this KniffelServiceSoapClient client, string rules)
        {
            var tcs = new TaskCompletionSource<GetTopPlayersResponse>();
            client.GetTopPlayersCompleted += (s, e) =>
            {
                if (e.Error != null) tcs.SetException(e.Error);
                else if (e.Cancelled) tcs.SetCanceled();
                else tcs.SetResult(new GetTopPlayersResponse(new GetTopPlayersResponseBody(e.Result.Body.GetTopPlayersResult,e.Result.Body.Players)));
            };
            client.GetTopPlayersAsync(new GetTopPlayersRequest( new GetTopPlayersRequestBody  (rules, null)));
            return tcs.Task;
        }
        /// <summary>
        /// Task wrapper for GetArtifactsAsync() method
        /// </summary>
        public static Task<GetPlayersMagicsResponse> GetPlayersMagicsTaskAsync(this KniffelServiceSoapClient client,
            string username, string pass, int rolls, int manuals, int resets)
        {
            var tcs = new TaskCompletionSource<GetPlayersMagicsResponse>();
            client.GetPlayersMagicsCompleted += (s, e) =>
            {
                if (e.Error != null) tcs.SetException(e.Error);
                else if (e.Cancelled) tcs.SetCanceled();
                else tcs.SetResult(new GetPlayersMagicsResponse(new GetPlayersMagicsResponseBody(e.Result.Body.GetPlayersMagicsResult, e.Result.Body.rolls, e.Result.Body.manuals, e.Result.Body.resets)));
            };
            client.GetPlayersMagicsAsync(new GetPlayersMagicsRequest( new GetPlayersMagicsRequestBody   (username, pass, rolls, manuals, resets)));
            return tcs.Task;
        }
        /// <summary>
        /// Task wrapper for AddArtifactsAsync() method
        /// </summary>
        public static Task<AddPlayersMagicsResponse> AddPlayersMagicsTaskAsync(this KniffelServiceSoapClient client,
            string username, string pass, string rolls, string manuals, string resets)
        {
            var tcs = new TaskCompletionSource<AddPlayersMagicsResponse>();
            client.AddPlayersMagicsCompleted += (s, e) =>
            {
                if (e.Error != null) tcs.SetException(e.Error);
                else if (e.Cancelled) tcs.SetCanceled();
                else tcs.SetResult(new AddPlayersMagicsResponse(new AddPlayersMagicsResponseBody(e.Result.Body.AddPlayersMagicsResult)));
            };
            client.AddPlayersMagicsAsync(new AddPlayersMagicsRequest(new AddPlayersMagicsRequestBody (username, pass, rolls, manuals, resets)));
            return tcs.Task;
        }

        /// <summary>
        /// Task wrapper for PutScoreAsync() method
        /// </summary>
        public static Task<PutScoreIntoTableWithPicPureNameResponse> PutScoreIntoTableWithPicPureNameTaskAsync(this KniffelServiceSoapClient client,
            string username, string pass, string score, string table, string picurl)
        {
            var tcs = new TaskCompletionSource<PutScoreIntoTableWithPicPureNameResponse>();
            client.PutScoreIntoTableWithPicPureNameCompleted += (s, e) =>
            {
                if (e.Error != null) tcs.SetException(e.Error);
                else if (e.Cancelled) tcs.SetCanceled();
                else tcs.SetResult(new PutScoreIntoTableWithPicPureNameResponse(new PutScoreIntoTableWithPicPureNameResponseBody(e.Result.Body.PutScoreIntoTableWithPicPureNameResult)));
            };
            client.PutScoreIntoTableWithPicPureNameAsync(new PutScoreIntoTableWithPicPureNameRequest(new PutScoreIntoTableWithPicPureNameRequestBody (username, pass, score, table, picurl)));
            return tcs.Task;
        }
    }
}
