using UnityEngine;

public class Gamemanager : MonoBehaviour
{
    public GameObject currentTurret;
    public GameObject currentBipod;

    public Sprite currentTurretSprite;
    public Sprite currentBipodSprite;

    public Transform turretTiles;
    public LayerMask turretTileMask;

    // 🔹 Настройка цвета подсветки
    private readonly Color greenColor = new Color(0f, 1f, 0f, 0.7f);
    private readonly Color redColor = new Color(1f, 0f, 0f, 0.7f);

    private void Update()
    {
        // Проверяем, есть ли клик по клетке
        RaycastHit2D hit = Physics2D.Raycast(
            Camera.main.ScreenToWorldPoint(Input.mousePosition),
            Vector2.zero,
            Mathf.Infinity,
            turretTileMask
        );

        // 🔹 Скрываем спрайты-подсветки со всех клеток
        foreach (Transform tile in turretTiles)
        {
            var sr = tile.GetComponent<SpriteRenderer>();
            if (sr) sr.enabled = false;
        }

        if (hit.collider)
        {
            var tile = hit.collider.GetComponent<TurretTile>();
            var sr = hit.collider.GetComponent<SpriteRenderer>();

            if (tile == null || sr == null)
                return;

            // УСТАНОВКА СОШЕК
            if (currentBipod)
            {
                sr.enabled = true;
                sr.sprite = currentBipodSprite;
                sr.transform.localScale = new Vector3(0.45f, 0.45f, 1f);

                if (!tile.hasBipod)
                    sr.color = greenColor;
                else
                    sr.color = redColor;

                if (Input.GetMouseButtonDown(0) && !tile.hasBipod)
                {
                    GameObject newBipod = Instantiate(currentBipod, tile.transform.position, Quaternion.identity);
                    tile.hasBipod = true;
                    tile.bipodObject = newBipod;

                    currentBipod = null;
                    currentBipodSprite = null;

                    TurretOffset bipodScript = newBipod.GetComponent<TurretOffset>();
                    if (bipodScript != null)
                        bipodScript.ApplyPlacementOffset();
                }
            }

            // УСТАНОВКА ТУРЕЛИ
            else if (currentTurret)
            {
                sr.enabled = true;
                sr.sprite = currentTurretSprite;
                sr.transform.localScale = new Vector3(0.35f, 0.35f, 1f);

                if (tile.hasBipod && !tile.hasTurret)
                    sr.color = greenColor;
                else
                    sr.color = redColor;  

                if (Input.GetMouseButtonDown(0) && tile.hasBipod && !tile.hasTurret)
                {
                    GameObject newTurret = Instantiate(currentTurret, tile.transform.position, Quaternion.identity);
                    tile.hasTurret = true;
                    tile.turretObject = newTurret;

                    currentTurret = null;
                    currentTurretSprite = null;

                    TurretOffset turretScript = newTurret.GetComponent<TurretOffset>();
                    if (turretScript != null)
                        turretScript.ApplyPlacementOffset();
                }
            }
        }
    }
}
