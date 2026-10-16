namespace USPK_Project.IServices;

// Implement against the team's approved role source. No role list is embedded here.
public interface IRoleCatalog
{
    Task<bool> CanAssignAsync(string role, CancellationToken cancellationToken);
}
