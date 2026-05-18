using UnityEngine;

/// <summary>
/// Floats a TextMesh upward and fades it out, then self-destructs.
/// Used for death emoji pop-up labels.
/// </summary>
public class FloatingText : MonoBehaviour
{
    float _life;
    float _elapsed;
    TextMesh _tm;
    MeshRenderer _mr;

    public void Init(float life)
    {
        _life = Mathf.Max(life, 0.05f);
        _tm   = GetComponent<TextMesh>();
        _mr   = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        _elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsed / _life);

        // Float upward
        transform.position += Vector3.up * Time.deltaTime * 0.8f;

        // Fade
        if (_tm != null)
        {
            var c = _tm.color;
            c.a = Mathf.Lerp(1f, 0f, t);
            _tm.color = c;
        }

        if (_elapsed >= _life)
            Destroy(gameObject);
    }
}
