using Godot;

public partial class GameManager : Node
{
    public Player playerInstance { get; private set; }

    public Field fieldInstance { get; private set; }  

    public static void RegisterPlayerInstance( Player newInstance ) { Instance.playerInstance = newInstance; }
    public static Player GetPlayer() { return Instance.playerInstance; }
    public static void RegisterFieldInstance( Field newInstance) { Instance.fieldInstance = newInstance; }  
    public static Field GetField() { return Instance.fieldInstance; }
}