using UnityEngine;
using System;

public class Dot : MonoBehaviour
{
    public int colorId;
    public Vector2Int gridPos;
    public Enum.DotState dotState;

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
    }

    public void OnSelected()
    {
        m_anim.SetTrigger("Click");
    }
}
