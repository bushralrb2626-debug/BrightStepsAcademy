using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace BrightStepsAcademy.Domain;

public static class AssessmentTypeCatalog
{
    public static readonly AssessmentType[] GradeBookOptions =
    [
        AssessmentType.Test,
        AssessmentType.BiMonthly,
        AssessmentType.Midterm,
        AssessmentType.FinalExam
    ];

    public static string Label(AssessmentType type) =>
        type.GetType().GetField(type.ToString())?.GetCustomAttribute<DisplayAttribute>()?.Name
        ?? type.ToString();

    public static string DefaultName(AssessmentType type, DateOnly date) =>
        $"{Label(type)} · {date:MMM d, yyyy}";
}
