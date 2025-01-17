using Unity.Entities;

public struct GameSettingsComponent : IComponentData
{
    public float asteroidVelocity;
    public float playerForce;
    public float bulletVelocity;
    public int numAsteroids;
    public int numBlocks;
    public int levelWidth;
    public int levelHeight;
    public int levelDepth;
    public float relevancyRadius;
}