namespace AnaliseDocumental.ApiCadastro.Dominio.Entidades;

public sealed class CadastroDocumental
{
    public Guid Id { get; private set; }
    public Guid DocumentoId { get; private set; }
    public string NomeCompleto { get; private set; }
    public string Cpf { get; private set; }
    public string Email { get; private set; }
    public string Bucket { get; private set; }
    public string ChaveS3 { get; private set; }
    public string Status { get; private set; }
    public DateTimeOffset CriadoEm { get; private set; }

    private CadastroDocumental(
        Guid id,
        Guid documentoId,
        string nomeCompleto,
        string cpf,
        string email,
        string bucket,
        string chaveS3,
        string status,
        DateTimeOffset criadoEm)
    {
        Id = id;
        DocumentoId = documentoId;
        NomeCompleto = nomeCompleto;
        Cpf = cpf;
        Email = email;
        Bucket = bucket;
        ChaveS3 = chaveS3;
        Status = status;
        CriadoEm = criadoEm;
    }

    public static CadastroDocumental Criar(
        string nomeCompleto,
        string cpf,
        string email,
        string bucket,
        string chaveS3)
    {
        return new CadastroDocumental(
            id: Guid.NewGuid(),
            documentoId: Guid.NewGuid(),
            nomeCompleto: nomeCompleto,
            cpf: cpf,
            email: email,
            bucket: bucket,
            chaveS3: chaveS3,
            status: "Recebido",
            criadoEm: DateTimeOffset.UtcNow);
    }
}