namespace MCP.Server.Models;

public class FormPatchResult
{
    public string Type { get; set; } = "form_patch";

    public string? Target { get; set; }

    public Dictionary<string, object?> Data { get; set; } = new();
}

public class FormPatchData
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? TcNo { get; set; }

    public string? Email { get; set; }

    public string? Branch { get; set; }

    public string? MotherName { get; set; }

    public string? FatherName { get; set; }

    public string? BirthDate { get; set; }
}