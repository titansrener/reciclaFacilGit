namespace ReciclaFacil.Domain.Common;

public enum CollectionStatus
{
    Waiting,
    InProgress,
    Finished
}

public static class CollectionStatusExtensions
{
    public static CollectionStatus FromLegacyCode(string value) => value switch
    {
        "A" => CollectionStatus.Waiting,
        "I" => CollectionStatus.InProgress,
        "F" => CollectionStatus.Finished,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Status de coleta inválido.")
    };
}
