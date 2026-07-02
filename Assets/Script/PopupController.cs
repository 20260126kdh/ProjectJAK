using UnityEngine;

public class PopupController : MonoBehaviour
{
    [SerializeField] GameObject popup;  // °ü¸®ÇÒ ÆË¾÷ ¿ÀºêÁ§Æ®

    void Start()
    {
        popup.SetActive(false);  // ½ÃÀÛ ½Ã ¼û±è
    }

    public void Open() => popup.SetActive(true);
    public void Close() => popup.SetActive(false);
}