namespace OOPGames
{
    // Plays generated sound clips. Several different clips may play at the same time;
    // playing a clip again restarts it.
    public interface IG10_ClipOutput
    {
        // False while a clip is still being loaded; playing it then has no effect.
        bool IsReady(G10_SoundClip clip);
        void Play(G10_SoundClip clip);
        void Stop(G10_SoundClip clip);
        void StopAll();
    }
}
