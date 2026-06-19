using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Api.Dto;

public static class DtoMapper
{
    private static readonly ConcurrentDictionary<(Type source, Type destination), Delegate> MapperCache = new();
    private static readonly HashSet<string> MappingWarnings = new();

    static DtoMapper()
    {
        InitializeMappers();
    }

    /// <summary>
    ///     Maps an object from TSource to TDest using reflection-compiled delegates.
    ///     Properties with matching names and types are automatically mapped.
    /// </summary>
    /// <typeparam name="TSource">Source object type</typeparam>
    /// <typeparam name="TDest">Destination object type</typeparam>
    /// <param name="source">Source object to map</param>
    /// <returns>Mapped destination object</returns>
    public static TDest Map<TSource, TDest>(TSource source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        (Type, Type) key = (typeof(TSource), typeof(TDest));
        Delegate mapper = MapperCache.GetOrAdd(key, BuildMapper);

        Func<TSource, TDest> mapperFunc = (Func<TSource, TDest>)mapper;
        return mapperFunc(source);
    }

    /// <summary>
    ///     Initializes all mappers for types marked with MappedObjectAttribute.
    ///     Called once at startup to build all mapper delegates.
    /// </summary>
    private static void InitializeMappers()
    {
        Assembly assembly = typeof(DtoMapper).Assembly;
        List<Type> typesWithMapping = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<MappedObjectAttribute>() != null)
            .ToList();

        foreach (Type type in typesWithMapping)
        {
            MappedObjectAttribute? attr = type.GetCustomAttribute<MappedObjectAttribute>();
            if (attr != null)
            {
                // Build bidirectional mappers
                MethodInfo? sourceToDestMethod = typeof(DtoMapper).GetMethod(
                    nameof(BuildAndCacheMapper),
                    BindingFlags.NonPublic | BindingFlags.Static);

                sourceToDestMethod!.MakeGenericMethod(type, attr.Other).Invoke(null, Array.Empty<object>());
                sourceToDestMethod!.MakeGenericMethod(attr.Other, type).Invoke(null, Array.Empty<object>());
            }
        }
    }

    /// <summary>
    ///     Builds and caches a mapper delegate for the given types.
    /// </summary>
    private static void BuildAndCacheMapper<TSource, TDest>() where TDest : new()
    {
        (Type, Type) key = (typeof(TSource), typeof(TDest));
        if (!MapperCache.ContainsKey(key))
        {
            Func<TSource, TDest> mapper = BuildMapper<TSource, TDest>(key);
            MapperCache.TryAdd(key, mapper);
        }
    }


    /// <summary>
    ///     Builds a compiled mapper delegate using expression trees.
    /// </summary>
    private static Func<TSource, TDest> BuildMapper<TSource, TDest>((Type source, Type destination) key)
        where TDest : new()
    {
        Type sourceType = key.source;
        Type destType = key.destination;

        ParameterExpression sourceParam = Expression.Parameter(sourceType, "source");
        ParameterExpression destVar = Expression.Variable(destType, "destination");

        List<Expression> expressions = new()
        {
            Expression.Assign(destVar, Expression.New(destType))
        };

        List<PropertyInfo> sourceProps = sourceType.GetProperties(
                BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToList();

        List<PropertyInfo> destProps = destType.GetProperties(
                BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToList();

        HashSet<string> mappedDestProps = new(StringComparer.OrdinalIgnoreCase);

        foreach (PropertyInfo sourceProp in sourceProps)
        {
            PropertyInfo? destProp = destProps.FirstOrDefault(p =>
                p.Name.Equals(sourceProp.Name, StringComparison.OrdinalIgnoreCase) &&
                p.PropertyType == sourceProp.PropertyType);

            if (destProp != null)
            {
                MemberExpression sourceValue = Expression.Property(sourceParam, sourceProp);
                BinaryExpression assignment = Expression.Assign(
                    Expression.Property(destVar, destProp),
                    sourceValue);

                expressions.Add(assignment);
                mappedDestProps.Add(destProp.Name);
            }
        }

        // Warn about unmapped destination properties
        List<PropertyInfo> unmappedProps = destProps.Where(p => !mappedDestProps.Contains(p.Name)).ToList();
        if (unmappedProps.Any())
        {
            string warningKey = $"{sourceType.Name} -> {destType.Name}";
            if (MappingWarnings.Add(warningKey))
            {
                string unmappedNames = string.Join(", ", unmappedProps.Select(p => p.Name));
                Debug.WriteLine(
                    $"[DtoMapper Warning] Unmapped properties in {destType.Name} when mapping from {sourceType.Name}: {unmappedNames}");
            }
        }

        expressions.Add(destVar);

        BlockExpression body = Expression.Block(new[] { destVar }, expressions);
        Expression<Func<TSource, TDest>> lambda = Expression.Lambda<Func<TSource, TDest>>(body, sourceParam);

        return lambda.Compile();
    }

    /// <summary>
    ///     Builds a mapper for any two types without using expression trees.
    ///     Called when types aren't known at compile time.
    /// </summary>
    private static Delegate BuildMapper((Type source, Type destination) key)
    {
        Type sourceType = key.source;
        Type destType = key.destination;

        ParameterExpression sourceParam = Expression.Parameter(sourceType, "source");
        ParameterExpression destVar = Expression.Variable(destType, "destination");

        List<Expression> expressions = new()
        {
            Expression.Assign(destVar, Expression.New(destType))
        };

        List<PropertyInfo> sourceProps = sourceType.GetProperties(
                BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToList();

        List<PropertyInfo> destProps = destType.GetProperties(
                BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToList();

        HashSet<string> mappedDestProps = new(StringComparer.OrdinalIgnoreCase);

        foreach (PropertyInfo sourceProp in sourceProps)
        {
            PropertyInfo? destProp = destProps.FirstOrDefault(p =>
                p.Name.Equals(sourceProp.Name, StringComparison.OrdinalIgnoreCase) &&
                p.PropertyType == sourceProp.PropertyType);

            if (destProp != null)
            {
                MemberExpression sourceValue = Expression.Property(sourceParam, sourceProp);
                BinaryExpression assignment = Expression.Assign(
                    Expression.Property(destVar, destProp),
                    sourceValue);

                expressions.Add(assignment);
                mappedDestProps.Add(destProp.Name);
            }
        }

        // Warn about unmapped destination properties
        List<PropertyInfo> unmappedProps = destProps.Where(p => !mappedDestProps.Contains(p.Name)).ToList();
        if (unmappedProps.Count != 0)
        {
            string warningKey = $"{sourceType.Name} -> {destType.Name}";
            if (MappingWarnings.Add(warningKey))
            {
                string unmappedNames = string.Join(", ", unmappedProps.Select(p => p.Name));
                Debug.WriteLine(
                    $"[DtoMapper Warning] Unmapped properties in {destType.Name} when mapping from {sourceType.Name}: {unmappedNames}");
            }
        }

        expressions.Add(destVar);

        BlockExpression body = Expression.Block([destVar], expressions);
        Type funcType = typeof(Func<,>).MakeGenericType(sourceType, destType);
        LambdaExpression lambda = Expression.Lambda(funcType, body, sourceParam);

        return lambda.Compile();
    }
}

/// <summary>
///     Denotes an object that is mapped to a database model. This is used to automatically generate mapping code between
///     the two types.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class MappedObjectAttribute(Type self, Type other) : Attribute
{
    public Type Self { get; } = self;
    public Type Other { get; } = other;
}

/// <summary>
/// Marks a property for explicit mapping between different property names or types.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ExplicitPropMapAttribute(string sourceProp, string destProp) : Attribute
{
    public string SourceProp { get; } = sourceProp;
    public string DestProp { get; } = destProp;
}

/// <summary>
/// Suppresses unmapped property warnings for this specific property.
/// Use when a property intentionally has no mapping.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SuppressUnmappedWarningAttribute : Attribute;