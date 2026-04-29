using UnityEngine;
using System.Collections;
using System;

public class WorldTime : MonoBehaviour
{
    public event EventHandler<TimeSpan> WorldTimeChanged;

    [SerializeField]
    private float _dayLength = 28800f; // 20s th?c = 1 phút game  (20 × 1440 = 28800)


    private TimeSpan _currentTime;
    private float _minuteLength => _dayLength / WorldTimeConstants.MinutesInDay;

    private void Start()
    {
        // Default start time: 07:00 if no save data has set the time yet
        if (_currentTime.Ticks == 0)
            _currentTime = TimeSpan.FromHours(7);

        WorldTimeChanged?.Invoke(this, _currentTime);
        StartCoroutine(AddMinute());
    }

    private IEnumerator AddMinute()
    {
        _currentTime += TimeSpan.FromMinutes(1);
        WorldTimeChanged?.Invoke(this, _currentTime);
        yield return new WaitForSeconds(_minuteLength);
        StartCoroutine(AddMinute());
    }

    public void AddMinutes(int minutes)
    {
        _currentTime += TimeSpan.FromMinutes(minutes);
        WorldTimeChanged?.Invoke(this, _currentTime);
    }

    public long GetCurrentTimeTicks() => _currentTime.Ticks;

    public void SetCurrentTime(long ticks)
    {
        _currentTime = TimeSpan.FromTicks(ticks);
        WorldTimeChanged?.Invoke(this, _currentTime);
    }
}
