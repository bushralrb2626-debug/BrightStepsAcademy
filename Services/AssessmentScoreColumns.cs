using System.Text.Json;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Models;

namespace BrightStepsAcademy.Services;

public static class AssessmentScoreColumns
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static List<AssessmentScoreColumnVm> Default(decimal maxMarks = 100) =>
    [
        new AssessmentScoreColumnVm
        {
            Key = "marks",
            AssessmentType = AssessmentType.Test,
            Name = AssessmentTypeCatalog.Label(AssessmentType.Test),
            MaxMarks = maxMarks
        }
    ];

    public static List<AssessmentScoreColumnVm> Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Default();

        try
        {
            var columns = JsonSerializer.Deserialize<List<AssessmentScoreColumnVm>>(json, JsonOptions);
            if (columns is not { Count: > 0 })
                return Default();

            foreach (var col in columns)
            {
                if (!AssessmentTypeCatalog.GradeBookOptions.Contains(col.AssessmentType)
                    && !string.IsNullOrWhiteSpace(col.Name))
                {
                    var matched = AssessmentTypeCatalog.GradeBookOptions
                        .FirstOrDefault(t => string.Equals(AssessmentTypeCatalog.Label(t), col.Name.Trim(), StringComparison.OrdinalIgnoreCase));
                    col.AssessmentType = matched != AssessmentType.Quiz ? matched : AssessmentType.Test;
                }
            }

            return columns;
        }
        catch
        {
            return Default();
        }
    }

    public static string Serialize(IReadOnlyList<AssessmentScoreColumnVm> columns) =>
        JsonSerializer.Serialize(columns, JsonOptions);

    public static List<decimal> BreakdownToScores(string? json, IReadOnlyList<AssessmentScoreColumnVm> columns)
    {
        if (columns.Count == 0)
            return [];

        var scores = columns.Select(_ => 0m).ToList();
        if (string.IsNullOrWhiteSpace(json))
            return scores;

        try
        {
            var map = JsonSerializer.Deserialize<Dictionary<string, decimal>>(json, JsonOptions);
            if (map is null)
                return scores;

            for (var i = 0; i < columns.Count; i++)
            {
                if (map.TryGetValue(columns[i].Key, out var value))
                    scores[i] = value;
            }
        }
        catch
        {
            // ignore malformed json
        }

        return scores;
    }

    public static string ScoresToBreakdown(IReadOnlyList<decimal> scores, IReadOnlyList<AssessmentScoreColumnVm> columns)
    {
        var map = new Dictionary<string, decimal>();
        for (var i = 0; i < columns.Count; i++)
            map[columns[i].Key] = i < scores.Count ? scores[i] : 0m;
        return JsonSerializer.Serialize(map, JsonOptions);
    }

    public static decimal TotalMax(IReadOnlyList<AssessmentScoreColumnVm> columns) =>
        columns.Sum(c => c.MaxMarks);

    public static decimal TotalObtained(IReadOnlyList<decimal> scores) =>
        scores.Sum();

    public static void NormalizeKeys(IList<AssessmentScoreColumnVm> columns)
    {
        for (var i = 0; i < columns.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(columns[i].Key))
                columns[i].Key = $"col{i}";
            columns[i].Name = AssessmentTypeCatalog.Label(columns[i].AssessmentType);
            if (columns[i].MaxMarks < 0)
                columns[i].MaxMarks = 0;
        }
    }

    public static void EnsureRowScores(AssessmentMarkRowVm row, int columnCount)
    {
        row.ColumnScores ??= [];
        while (row.ColumnScores.Count < columnCount)
            row.ColumnScores.Add(0);
        if (row.ColumnScores.Count > columnCount)
            row.ColumnScores = row.ColumnScores.Take(columnCount).ToList();
    }

    public static string TitleFromColumns(IReadOnlyList<AssessmentScoreColumnVm> columns, DateOnly date)
    {
        if (columns.Count == 0)
            return AssessmentTypeCatalog.DefaultName(AssessmentType.Test, date);
        if (columns.Count == 1)
            return AssessmentTypeCatalog.DefaultName(columns[0].AssessmentType, date);
        var parts = string.Join(" + ", columns.Select(c => AssessmentTypeCatalog.Label(c.AssessmentType)));
        return $"{parts} · {date:MMM d, yyyy}";
    }

    public static void ApplyAssessmentDefaults(AssessmentFormVm model)
    {
        NormalizeKeys(model.Columns);
        if (model.Columns.Count > 0)
            model.AssessmentType = model.Columns[0].AssessmentType;
        if (model.AssessmentDate == default)
            model.AssessmentDate = DateOnly.FromDateTime(DateTime.Today);
        if (string.IsNullOrWhiteSpace(model.Name))
            model.Name = TitleFromColumns(model.Columns, model.AssessmentDate);
        if (model.PassingMarks <= 0)
            model.PassingMarks = 40;
        model.TotalMarks = TotalMax(model.Columns);
    }

    public static void ApplyAssessmentDefaults(AssessmentMarksFormVm model, Assessment? existing = null)
    {
        NormalizeKeys(model.Columns);
        if (model.Columns.Count > 0)
            model.AssessmentType = model.Columns[0].AssessmentType;
        else if (existing is not null)
            model.AssessmentType = existing.AssessmentType;

        if (model.AssessmentDate == default)
            model.AssessmentDate = existing?.AssessmentDate ?? DateOnly.FromDateTime(DateTime.Today);

        if (string.IsNullOrWhiteSpace(model.Name))
            model.Name = TitleFromColumns(model.Columns, model.AssessmentDate);

        if (model.PassingMarks <= 0)
            model.PassingMarks = existing?.PassingMarks ?? 40;

        model.TotalMarks = TotalMax(model.Columns);
    }
}
