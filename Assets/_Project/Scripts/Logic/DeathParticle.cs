using UnityEngine;

/// <summary>
/// Attached to a short-lived particle spawned on enemy death.
/// Moves outward, shrinks, fades, then self-destructs.
/// </summary>
public class DeathParticle : MonoBehaviour
{
    Vector2 _velocity;
    float   _life;
    float   _elapsed;
    Color   _startColor;
    SpriteRenderer _sr;

    public void Init(Vector2 velocity, float life, Color color)
    {
        _velocity   = velocity;
        _life       = Mathf.Max(life, 0.05f);
        _startColor = color;
        _sr         = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        _elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsed / _life);

        // Move & decelerate
        transform.position += (Vector3)(_velocity * Time.deltaTime);
        _velocity *= Mathf.Pow(0.05f, Time.deltaTime); // drag

        // Shrink
        float scale = Mathf.Lerp(1f, 0f, t);
        transform.localScale = Vector3.one * scale;

        // Fade
        if (_sr != null)
        {
            var c = _startColor;
            c.a = Mathf.Lerp(_startColor.a, 0f, t);
            _sr.color = c;
        }

        if (_elapsed >= _life)
            Destroy(gameObject);
    }
}
