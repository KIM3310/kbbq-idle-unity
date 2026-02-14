#if UNITY_EDITOR
using NUnit.Framework;

public class DataValidationTests
{
    [Test]
    public void ProjectData_HasNoValidationErrors()
    {
        var report = KBBQDataValidator.Validate();
        Assert.IsFalse(report.HasErrors, "Validation errors found. Open Console or run KBBQ/Validate Data (Portfolio).");
    }
}
#endif

