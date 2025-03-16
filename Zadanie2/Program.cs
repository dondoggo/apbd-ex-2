// See https://aka.ms/new-console-template for more information

public class Container
{
    public enum container_type { 
        L, G, C
    };
    
    int cargo_mass;
    int container_mass;
    int height;
    int depth;
    int max_load;

    private string serial_number;
    string sn_part_1 = "KON";
    char sn_separator = '-';
    private static int sn_unique_number = 0;

    public Container(container_type type, int cargoMass, int containerMass, int height, int depth, int maxLoad)
    {
        this.cargo_mass = cargoMass;
        this.container_mass = containerMass;
        this.height = height;
        this.depth = depth;
        this.max_load = maxLoad;
        sn_unique_number++;
        this.serial_number = sn_part_1 + sn_separator + type + this.sn_separator + sn_unique_number.ToString();
    }

    public override string ToString()
    {
        return $"Container {serial_number} \nType: {this.GetType().Name} \nCargo Mass: {cargo_mass}kg \nContainer Mass: {container_mass}kg \nHeight: {height}m \nDepth: {depth}m \nMax Load: {max_load}kg";
    }


    public static void Main(string[] args)
    {
        Container c1 = new Container(container_type.L, 3, 3, 3, 3, 3);
        Container c2 = new Container(container_type.G, 3, 3, 3, 3, 3);
        Console.WriteLine(c1.ToString());
        Console.WriteLine(c2.ToString());
    }
}


