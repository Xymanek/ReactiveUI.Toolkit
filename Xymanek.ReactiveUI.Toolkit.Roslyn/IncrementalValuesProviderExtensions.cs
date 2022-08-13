using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Xymanek.ReactiveUI.Toolkit.Roslyn;

public static class IncrementalValuesProviderExtensions
{
    // Based on (with a few API changes)
    // https://github.com/CommunityToolkit/dotnet/blob/96517eace88d95ec52a881e3e1ed74ea51436b40/CommunityToolkit.Mvvm.SourceGenerators/Extensions/IncrementalValuesProviderExtensions.cs#L20-L58
    public static IncrementalValuesProvider<(TGroup Group, ImmutableArray<TItem> Items)> Group<TGroup, TItem>(
        this IncrementalValuesProvider<(TGroup Group, TItem Item)> source,
        IEqualityComparer<TGroup>? comparer = null
    )
    {
        comparer ??= EqualityComparer<TGroup>.Default;
        
        return source.Collect().SelectMany((item, _) =>
        {
            Dictionary<TGroup, ImmutableArray<TItem>.Builder> map = new(comparer);

            foreach ((TGroup hierarchy, TItem info) in item)
            {
                if (!map.TryGetValue(hierarchy, out ImmutableArray<TItem>.Builder builder))
                {
                    builder = ImmutableArray.CreateBuilder<TItem>();

                    map.Add(hierarchy, builder);
                }

                builder.Add(info);
            }

            ImmutableArray<(TGroup Hierarchy, ImmutableArray<TItem> Properties)>.Builder result =
                ImmutableArray.CreateBuilder<(TGroup, ImmutableArray<TItem>)>();

            foreach (KeyValuePair<TGroup, ImmutableArray<TItem>.Builder> entry in map)
            {
                result.Add((entry.Key, entry.Value.ToImmutable()));
            }

            return result;
        });
    }
}