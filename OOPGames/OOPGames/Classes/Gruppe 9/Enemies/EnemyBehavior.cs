namespace OOPGames
{
    // How an enemy moves towards its target (see PursuerEnemy).
    public enum EnemyBehavior
    {
        // Walks to the target until the hitboxes touch (the original enemy).
        Chaser,
        // Keeps PreferredDistance to the target and backs away when it comes closer.
        Ranged,
        // Does not move; turns to the target and attacks in reach.
        Turret,
        // Like Chaser, but flees below FleeBelowHealthPercent of its hit points.
        Coward
    }

    // Whom an enemy follows: the player and mirror images in sight (with alert), or a
    // random living entity of any kind anywhere (player, mirror image or other enemy).
    public enum EnemyTargetSelection
    {
        Player,
        RandomEntity
    }

    // Walking moves steadily; hopping moves in short hops with pauses in between.
    public enum EnemyMovementStyle
    {
        Walk,
        Hop
    }

    // What starts an attack: a target within its distance (default) or a target whose
    // hitbox touches the enemy's hitbox (e.g. stepping on a mine).
    public enum EnemyAttackTrigger
    {
        Distance,
        Contact
    }

    // Who may set an attack off: whoever it targets (default), only the player, the
    // player's side (player and mirror images) or anyone.
    public enum EnemyTriggerSource
    {
        Targets,
        Player,
        PlayerSide,
        Anyone
    }

    // When a kind may spawn, measured against its spawn regions on the played map. Kinds
    // without regions there ignore the condition.
    public enum EnemySpawnPlayerCondition
    {
        Always,
        PlayerInsideRegion,
        PlayerOutsideRegion
    }
}
