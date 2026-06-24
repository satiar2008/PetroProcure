namespace PetroProcure.AI;

public static class AiContextFactory
{
    public static IReadOnlyList<AiContextItemGroup> GroupItems(IEnumerable<(string MescCode, string GeneralCode, string GeneralDescription, string SpecificDescription, string Unit, decimal Quantity)> items) =>
        items.GroupBy(x => new { x.GeneralCode, x.GeneralDescription }).OrderBy(x => x.Key.GeneralCode)
            .Select(g => new AiContextItemGroup(g.Key.GeneralCode, g.Key.GeneralDescription,
                g.Select(x => new AiContextItem(x.MescCode, x.SpecificDescription, x.Unit, x.Quantity)).ToArray())).ToArray();
}
