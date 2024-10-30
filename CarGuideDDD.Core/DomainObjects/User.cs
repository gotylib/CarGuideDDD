namespace CarGuideDDD.Core.DomainObjects
{
    public class User 
    {
        public string? Name {  get; private set; }
        public string? Email { get; private set; }
        public string? Role { get; private set; }
        public string? Password { get; private set; }
        
        public void Create(string name, string email,string role, string password )
        {
            Name = name;
            Email = email;
            Role = role;
            Password = password;
        }

    }
    
}
