// Created By Levi Enama
using System;
using System.Collections.Generic;

namespace OpenEngine.Core.Timeline
{
    public class Timeline
    {
        public string Name { get; set; }
        public float Duration { get; set; }
        public float CurrentTime { get; set; }
        public PlaybackState State { get; set; }
        public float PlaybackSpeed { get; set; }
        public List<TimelineTrack> Tracks { get; set; }
        public List<Marker> Markers { get; set; }

        public Timeline()
        {
            Duration = 10f;
            CurrentTime = 0f;
            State = PlaybackState.Stopped;
            PlaybackSpeed = 1f;
            Tracks = new List<TimelineTrack>();
            Markers = new List<Marker>();
        }

        public void Play()
        {
            State = PlaybackState.Playing;
        }

        public void Pause()
        {
            State = PlaybackState.Paused;
        }

        public void Stop()
        {
            State = PlaybackState.Stopped;
            CurrentTime = 0f;
        }

        public void Update(float deltaTime)
        {
            if (State == PlaybackState.Playing)
            {
                CurrentTime += deltaTime * PlaybackSpeed;
                
                if (CurrentTime >= Duration)
                {
                    CurrentTime = 0f;
                    State = PlaybackState.Stopped;
                }

                EvaluateTracks();
            }
        }

        private void EvaluateTracks()
        {
            foreach (var track in Tracks)
            {
                track.Evaluate(CurrentTime);
            }
        }

        public void AddTrack(TimelineTrack track)
        {
            Tracks.Add(track);
        }

        public void RemoveTrack(TimelineTrack track)
        {
            Tracks.Remove(track);
        }

        public void AddMarker(Marker marker)
        {
            Markers.Add(marker);
        }

        public void SetTime(float time)
        {
            CurrentTime = Math.Clamp(time, 0f, Duration);
        }

        public void SetPlaybackSpeed(float speed)
        {
            PlaybackSpeed = Math.Max(0f, speed);
        }
    }

    public enum PlaybackState { Stopped, Playing, Paused }

    public abstract class TimelineTrack
    {
        public string Name { get; set; }
        public bool Muted { get; set; }
        public bool Locked { get; set; }
        public List<TimelineClip> Clips { get; set; }

        protected TimelineTrack()
        {
            Clips = new List<TimelineClip>();
        }

        public abstract void Evaluate(float time);
    }

    public class ActivationTrack : TimelineTrack
    {
        public override void Evaluate(float time)
        {
            foreach (var clip in Clips)
            {
                if (time >= clip.StartTime && time <= clip.EndTime)
                {
                    clip.Evaluate(time);
                }
            }
        }
    }

    public class AnimationTrack : TimelineTrack
    {
        public string AvatarMask { get; set; }

        public override void Evaluate(float time)
        {
            foreach (var clip in Clips)
            {
                if (time >= clip.StartTime && time <= clip.EndTime)
                {
                    clip.Evaluate(time);
                }
            }
        }
    }

    public class AudioTrack : TimelineTrack
    {
        public override void Evaluate(float time)
        {
            foreach (var clip in Clips)
            {
                if (time >= clip.StartTime && time <= clip.EndTime)
                {
                    clip.Evaluate(time);
                }
            }
        }
    }

    public class TimelineClip
    {
        public string Name { get; set; }
        public float StartTime { get; set; }
        public float EndTime { get; set; }
        public float Duration => EndTime - StartTime;
        public object Asset { get; set; }
        public bool Loop { get; set; }
        public float Speed { get; set; }

        public TimelineClip()
        {
            Loop = false;
            Speed = 1f;
        }

        public virtual void Evaluate(float time)
        {
            var clipTime = (time - StartTime) * Speed;
            if (Loop)
            {
                clipTime = clipTime % Duration;
            }
        }
    }

    public class Marker
    {
        public string Name { get; set; }
        public float Time { get; set; }
        public string Color { get; set; }

        public Marker()
        {
            Color = "#FFFFFF";
        }
    }

    public class Director
    {
        private Timeline _currentTimeline;
        private Dictionary<string, Timeline> _timelines;

        public Director()
        {
            _timelines = new Dictionary<string, Timeline>();
        }

        public void PlayTimeline(string timelineName)
        {
            if (_timelines.ContainsKey(timelineName))
            {
                _currentTimeline = _timelines[timelineName];
                _currentTimeline.Play();
            }
        }

        public void PauseTimeline()
        {
            _currentTimeline?.Pause();
        }

        public void StopTimeline()
        {
            _currentTimeline?.Stop();
        }

        public void RegisterTimeline(Timeline timeline)
        {
            _timelines[timeline.Name] = timeline;
        }

        public void Update(float deltaTime)
        {
            _currentTimeline?.Update(deltaTime);
        }
    }
}
