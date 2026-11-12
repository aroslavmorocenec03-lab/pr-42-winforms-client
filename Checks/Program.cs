using System.Net;
using PostService.App;
using PostService.CommonTypes;
using PostService.Dtos;

int count = 0;
void Check(bool condition, string description)
{
    if (!condition) throw new Exception("FAIL: " + description);
    Console.WriteLine($"PASS {++count:00}: {description}");
}
Dictionary<string, string> Valid() => new()
{
    ["From"] = "Київ, відділення № 3", ["To"] = "Львів, відділення № 7", ["Content"] = "Книги",
    ["Weight"] = "2,5", ["Width"] = "20.5", ["Height"] = "15,25", ["Depth"] = "3",
    ["Value"] = "100,50", ["Price"] = "65", ["Description"] = "Не згинати"
};
Dictionary<string, string> Validate(Dictionary<string, string> f, bool edit = false, DeliveryType? type = DeliveryType.Department)
    => PostingInput.Validate(f, type, edit, out _, out _);

var fields = Valid();
Check(PostingInput.Validate(fields, DeliveryType.Department, false, out var post, out _) .Count == 0 && post.Weight == 2.5f && post.Width == 20.5f, "Comma and dot decimals accepted");
foreach (var key in new[] { "From", "To", "Content" })
{
    fields = Valid(); fields[key] = "  "; Check(Validate(fields).ContainsKey(key), $"Required {key}");
}
foreach (var key in new[] { "Weight", "Width", "Height", "Depth" })
{
    fields = Valid(); fields[key] = "0"; Check(Validate(fields).ContainsKey(key), $"Positive {key}");
}
foreach (var bad in new[] { "-1", "1000001", "NaN", "Infinity", "1e50", "abc" })
{
    fields = Valid(); fields["Weight"] = bad; Check(Validate(fields).ContainsKey("Weight"), $"Reject weight {bad}");
}
fields = Valid(); fields["Weight"] = "1000000";
Check(Validate(fields).Count == 0, "Weight upper boundary accepted");
fields = Valid(); fields["Value"] = ""; fields["Description"] = "";
Check(PostingInput.Validate(fields, DeliveryType.Courier, false, out post, out _).Count == 0 && post.Value is null && post.Description is null, "Optional fields become null");
fields["Value"] = "-1"; Check(Validate(fields).ContainsKey("Value"), "Negative declared value rejected");
fields = Valid(); fields["Price"] = "-1"; Check(Validate(fields, true).ContainsKey("Price"), "Negative edit price rejected");
Check(Validate(fields, false).Count == 0, "Create price is server owned");
Check(Validate(Valid(), type: null).ContainsKey("DeliveryType") && Validate(Valid(), type: (DeliveryType)99).ContainsKey("DeliveryType"), "Invalid delivery type rejected");
if (args.Length == 0) { Console.WriteLine($"All {count} checks passed. Pass URL for integration checks."); return; }

using var api = new PostServiceApi(args[0]);
var originalIds = (await api.GetAllAsync()).Select(x => x.Id).ToArray();
var createdIds = new List<int>();
try
{
    foreach (var type in Enum.GetValues<DeliveryType>())
    {
        PostingInput.Validate(Valid(), type, false, out post, out _);
        var created = await api.CreateAsync(post); createdIds.Add(created.Id);
        float expected = type switch { DeliveryType.Department => 65, DeliveryType.Courier => 117.5f, _ => 180 };
        Check(created.Id > 0 && created.Price == expected && created.CreatedAt != default, $"POST {type}, server price and timestamp");
        var loaded = await api.GetAsync(created.Id);
        Check(loaded.Content == post.Content && loaded.Weight == 2.5f && loaded.Value == 100.5f, $"GET by id {type} preserves data");
    }
    var first = await api.GetAsync(createdIds[0]);
    PostingInput.Validate(Valid(), DeliveryType.ExpressCourier, true, out _, out var put);
    put.Id = first.Id; put.Content = "Оновлені книги"; put.Price = 99.75f;
    var updated = await api.UpdateAsync(put);
    Check(updated.Content == put.Content && updated.Price == 99.75f && updated.CreatedAt == first.CreatedAt, "PUT changes fields and preserves CreatedAt");
    Check((await api.GetAllAsync()).Any(x => x.Id == put.Id && x.Content == put.Content), "List reflects PUT");
    put.Weight = 0;
    try { await api.UpdateAsync(put); Check(false, "Server must reject invalid input"); }
    catch (PostServiceApiException e) { Check(e.StatusCode == HttpStatusCode.BadRequest && e.Errors.ContainsKey("Weight"), "Server validation maps Weight error"); }
    Check((await api.GetAsync(put.Id)).Weight == first.Weight, "Rejected PUT leaves saved data unchanged");
    put.Weight = 2.5f; put.Id = first.Id + 999999;
    try { await api.UpdateAsync(put); Check(false, "Missing edit must be rejected"); }
    catch (PostServiceApiException e) { Check(e.StatusCode == HttpStatusCode.NotFound, "Missing record returns 404"); }
    await api.DeleteAsync(first.Id);
    Check(!(await api.GetAllAsync()).Any(x => x.Id == first.Id), "DELETE refreshes server list");
    try { await api.GetAsync(first.Id); Check(false, "Deleted record must be missing"); }
    catch (PostServiceApiException e) { Check(e.StatusCode == HttpStatusCode.NotFound, "GET deleted record returns 404"); }
}
finally
{
    foreach (var id in createdIds)
        try { await api.DeleteAsync(id); } catch (PostServiceApiException e) when (e.StatusCode == HttpStatusCode.NotFound) { }
}
Check((await api.GetAllAsync()).Select(x => x.Id).SequenceEqual(originalIds), "Integration fixtures cleaned, original records preserved");
Console.WriteLine($"All {count} checks passed.");
