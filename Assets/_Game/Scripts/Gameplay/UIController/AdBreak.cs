using System;
using System.Collections.Generic;
using MEC;
using TMPro;
using UnityEngine;

public class AdBreak : Singleton<AdBreak>
{
    [SerializeField] private TextMeshProUGUI _text;

    public IEnumerator<float> Countdown(Action action)
    {
        _text.text = "Ad-break in 3";
        yield return Timing.WaitForSeconds(1f);
        _text.text = "Ad-break in 2";
        yield return Timing.WaitForSeconds(1f);
        _text.text = "Ad-break in 1";
        yield return Timing.WaitForSeconds(1f);

        action.Invoke();
    }
}
