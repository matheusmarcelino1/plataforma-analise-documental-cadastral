using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AnaliseDocumental.ApiCadastro.Dominio.Entidades;

public sealed class CadastroDocumental
{
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; private set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid DocumentoId { get; private set; }
    public string NomeCompleto { get; private set; } = string.Empty;
    public string Cpf { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Bucket { get; private set; } = string.Empty;
    public string ChaveS3 { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public DateTimeOffset CriadoEm { get; private set; }

    private CadastroDocumental()
    {
    }

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
        Guid cadastroId,
        Guid documentoId,
        string nomeCompleto,
        string cpf,
        string email,
        string bucket,
        string chaveS3)
    {
        return new CadastroDocumental(
            id: cadastroId,
            documentoId: documentoId,
            nomeCompleto: nomeCompleto,
            cpf: cpf,
            email: email,
            bucket: bucket,
            chaveS3: chaveS3,
            status: "Recebido",
            criadoEm: DateTimeOffset.UtcNow);
    }
}