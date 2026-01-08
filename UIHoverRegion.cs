using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

public class UIHoverRegion : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    public SpriteShapeController hoverEffect;

    public float fadeSpeed = 8f;
    private float alpha = 0f;
    private bool hovering = false;

    void Start()
    {
        SetAlpha(0f);
        hoverEffect.gameObject.SetActive(true);
    }

    void Update()
    {
        float target = hovering ? 1f : 0f;
        alpha = Mathf.MoveTowards(alpha, target, Time.deltaTime * fadeSpeed);
        SetAlpha(alpha);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log(gameObject.name + " UI alanýna týklandý");
    }

    void SetAlpha(float a)
    {
        var sr = hoverEffect.GetComponent<SpriteShapeRenderer>();
        Color c = sr.color;
        c.a = a * 0.2f;
        sr.color = c;
    }
}
