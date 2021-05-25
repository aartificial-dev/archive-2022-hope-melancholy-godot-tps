using Godot;
using System;

public static class GameHelper {
    public static float delta = 0.01666667f;

    public static float GetDeltaValue(float value, float delta) {
        return (value / GameHelper.delta) * delta;
    }

    public static int AnimationPlayerAddAudioTrack(AnimationPlayer aplayer, String animation, String path) {
		int track = aplayer.GetAnimation(animation).AddTrack(Animation.TrackType.Audio);
		aplayer.GetAnimation(animation).TrackSetPath(track, path);
        return track;
    }

    public static void AnimationPlayerAddAudioKey(AnimationPlayer animationPlayer, String animation, int track, float time, Resource stream) {
		animationPlayer.GetAnimation(animation).AudioTrackInsertKey(track, time, stream);
    }

}
