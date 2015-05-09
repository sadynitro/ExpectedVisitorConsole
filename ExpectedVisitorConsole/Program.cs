using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ExpectedVisitorConsole
{

  public class StringTable
  {
    public string[] ColumnNames { get; set; }
    public string[,] Values { get; set; }
  }

  class Program
  {
    static readonly string[] weekdayMaster = { "月", "火", "水", "木", "金", "土", "日" };
    static readonly string[] weatherMaster = { "晴", "曇", "雨" };

    static void Main(string[] args)
    {
      InvokeRequestResponseService().Wait();
    }

    static async Task InvokeRequestResponseService()
    {
      string weekdayVal;
      do
      {
        Console.Write("曜日を入力 (月 火 水 木 金 土 日) => ");
        weekdayVal = Console.ReadLine();
      } while (!IsValidWeekday(weekdayVal));

      string weatherVal;
      do
      {
        Console.Write("天気を入力 (晴 曇 雨) => ");
        weatherVal = Console.ReadLine();
      } while (!IsValidWeather(weatherVal));

      Console.Write("気温を入力 (##.#) => ");
      var tempVal = Console.ReadLine();

      using (var client = new HttpClient())
      {
        var scoreRequest = new
        {

          Inputs = new Dictionary<string, StringTable>() { 
                        { 
                            "input1", 
                            new StringTable() 
                            {
                                ColumnNames = new string[] {"weekday", "weather", "temperature", "visitor"},
                                Values = new string[,] {  { weekdayVal, weatherVal, tempVal, "0" } }
                            }
                        },
                                        },
          GlobalParameters = new Dictionary<string, string>()
          {
          }
        };
        const string apiKey = "ここにWeb Serviceのトップ画面に表示されているAPIキーを入れます"; // Replace this with the API key for the web service
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        client.BaseAddress = new Uri("ここにREQUEST/RESPONSEのページに表示されているRequest URIを入れます");

        // WARNING: The 'await' statement below can result in a deadlock if you are calling this code from the UI thread of an ASP.Net application.
        // One way to address this would be to call ConfigureAwait(false) so that the execution does not attempt to resume on the original context.
        // For instance, replace code such as:
        //      result = await DoSomeTask()
        // with the following:
        //      result = await DoSomeTask().ConfigureAwait(false)


        HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest);

        if (response.IsSuccessStatusCode)
        {
          string result = await response.Content.ReadAsStringAsync();
          var jsonObj = JObject.Parse(result);
          var values = jsonObj["Results"]["output1"]["value"]["Values"].Children();
          foreach (var val in values)
          {
            Console.WriteLine("予測来館者数 => {0} 人", (int)(val[4].ToObject<double>()));
          }
        }
        else
        {
          Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

          // Print the headers - they include the requert ID and the timestamp, which are useful for debugging the failure
          Console.WriteLine(response.Headers.ToString());

          string responseContent = await response.Content.ReadAsStringAsync();
          Console.WriteLine(responseContent);
        }
      }
    }

    static bool IsValidWeekday(string input)
    {
      return weekdayMaster.Contains(input);

    }

    static bool IsValidWeather(string input)
    {
      return weatherMaster.Contains(input);
    }
  }
}

