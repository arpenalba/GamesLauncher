namespace GameLauncher.Models
{
    public class LaunchType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string LaunchCommand { get; set; }
        public string LaunchParams { get; set; }
    }
}
