using System.Collections;
using UnityEngine;
using TMPro;

public class TurnBanner : MonoBehaviour
{
    [SerializeField] TMP_Text playerText; // PLAYER TURN 텍스트
    [SerializeField] TMP_Text enemyText;  // ENEMY TURN 텍스트

    [SerializeField] float holdTime = 1.5f;   // 완전히 보이는 시간
    [SerializeField] float fadeTime = 0.5f;   // 사라지는 데 걸리는 시간

    void Awake()
    {
        // 시작 시 둘 다 투명하게
        SetAlpha(playerText, 0f);
        SetAlpha(enemyText, 0f);
    }

    public void ShowPlayer() => StartCoroutine(ShowBanner(playerText));
    public void ShowEnemy() => StartCoroutine(ShowBanner(enemyText));

    IEnumerator ShowBanner(TMP_Text text)
    {
        // 즉시 켜기
        SetAlpha(text, 1f);

        // 1.5초 유지
        yield return new WaitForSeconds(holdTime);

        // 페이드 아웃
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            SetAlpha(text, 1f - t / fadeTime);
            yield return null;
        }
        SetAlpha(text, 0f);
    }

    void SetAlpha(TMP_Text text, float a)
    {
        Color c = text.color;
        c.a = a;
        text.color = c;
    }
}