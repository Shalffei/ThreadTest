using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using RestSharp;

ConcurrentBag<string> keyWordsFromRequest = ParallelGetRequestToTestSud();
//File.WriteAllLines("D:\\WorkDirectory\\TestSud\\Responses.txt", result);
//var responsesFromRequest = File.ReadAllLines("D:\\WorkDirectory\\TestSud\\Responses.txt");
//ConcurrentBag<string> keyWordsFromRequest = new ConcurrentBag<string>();
//foreach (var line in responsesFromRequest)
//{
//    keyWordsFromRequest.Add(line);
//}
var resultFromDirectory = GetElementsFromText();
var result = GetContainsKeysInTxt(resultFromDirectory, keyWordsFromRequest).ToList();

Console.ReadLine();



ConcurrentBag<string> ParallelGetRequestToTestSud()
{
    WebProxy proxy = new WebProxy("195.123.189.44:2831", false);
    proxy.Credentials = new NetworkCredential("26414", "0I45pYUJ");
    RestClientOptions options = new RestClientOptions();
    options.Proxy = proxy;
    options.BaseUrl = new Uri("https://89.184.66.108:7478");
    RestClient client = new RestClient(options);
    List<string> ololoList = new List<string>();
    var counter = 0;
    ConcurrentBag<string> patentResponse = new ConcurrentBag<string>(); // потокобезопасная коллекция
    Parallel.For(1, 50000, new ParallelOptions() { MaxDegreeOfParallelism = 30000}, //делем перебор итемов в мнногопотоке, максимум 3 потока
        item =>
        {
            try
            {
                RestRequest request = new RestRequest("/test/TestSud", Method.Get);
                request.Timeout = 45000;
                RestResponse response = client.Execute(request);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine(response.StatusCode);
                    for (int i = 0; i < 5; i++)
                    {
                        if (response.IsSuccessful)
                        {
                            break;
                        }
                        response = client.Execute(request);
                    }
                }
                patentResponse.Add(response.Content.Trim('\"').Trim()); //записываем результат в потокобезопасную коллекцию
            }
            catch (Exception e)
            {
                //жопа
            }

            Interlocked.Increment(ref counter); // пишем сколько выполнили строк, потокобезопасным способом
            if (counter % 100 == 0)
            {
                Console.WriteLine(counter); //выводим в консоле сообщение за каждые 100 пройденых итемов
            }
        });
    return patentResponse;
}

ConcurrentBag<string> GetElementsFromText ()
{
    ConcurrentBag<string> filesText = new ConcurrentBag<string>();
    string[] filesPath = Directory.GetFiles("D:\\WorkDirectory\\ololo\\");
    Parallel.ForEach(filesPath,
        item =>
        {
            try
            {
                filesText.Add(File.ReadAllText(item));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        });
    return filesText;
}
ConcurrentDictionary<string, int> GetContainsKeysInTxt(ConcurrentBag<string> resultFromRequest, ConcurrentBag<string> keyWordsFromRequest)
{
    ConcurrentDictionary<string, int> keySubstringWithCounters = new ConcurrentDictionary<string, int>(); 

    Parallel.ForEach(resultFromRequest,
        item =>
        {
            try
            {
                foreach (var key in keyWordsFromRequest)
                {
                    if (item.Contains(key) && keySubstringWithCounters.TryGetValue(key, out int notNeeded) == false)
                    {
                        keySubstringWithCounters.TryAdd(key, item.Split(key).Length - 1);
                    }
                    else if (keySubstringWithCounters.TryGetValue(key, out int oldValue) == true)
                    {
                        keySubstringWithCounters.AddOrUpdate(key, oldValue, (key, oldValue) => oldValue + item.Split(key).Length - 1);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        });
    return keySubstringWithCounters;
}
class KeySubstringWithCounter
{
    public string Key { get; set; }
    public int Count { get; set; }
}


