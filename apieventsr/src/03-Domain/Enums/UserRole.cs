namespace apieventsr.Domain.Enums
{
    public enum UserRole
    {
        School = 1,      // Escola — acesso amplo
        Coordinator = 2, // Coordenador — acesso por segmento vinculado
        Professor = 3    // Professor — acesso apenas ao próprio projeto
    }
}
