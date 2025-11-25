using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    [SerializeField] private SpriteRenderer m_borderSpr;
    public int colorId;
    public Vector2Int gridPos;
    public Enum.DotState dotState;
    public List<Vector2Int> connection;
    private SpriteRenderer m_sprRdr;
    private Animator m_anim;
    private void Awake()
    {
        m_sprRdr = GetComponent<SpriteRenderer>();
        m_anim = GetComponent<Animator>();
    }
    public void SetColor(Color color)
    {
        if (m_sprRdr == null) return;
        m_sprRdr.color = color;
        Color tempColor = color;
        tempColor.a = 0.3f;
        m_borderSpr.color = tempColor;
    }

    public void OnSelected()
    {
        m_anim.SetTrigger("Click");
    }
}
