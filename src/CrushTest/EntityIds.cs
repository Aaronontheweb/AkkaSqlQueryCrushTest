﻿namespace CrushTest;

public static class EntityIds
{
    public static readonly string[] AllEntityIds = Enumerable.Range(0, 100_000).Select(i => $"entity-{i}").ToArray();
}