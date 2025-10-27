using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using Random = UnityEngine.Random;

public sealed class Board : MonoBehaviour
{
    public static Board Instance { get; private set; }

    [SerializeField] private AudioClip collectSound;
    [SerializeField] private AudioSource audioSource;
    public Row[] rows;
    
    public Tile[,] Tiles { get; private set; }
    
    public int Width => Tiles.GetLength(0);
    public int Height => Tiles.GetLength(1);

    private readonly List<Tile> _selection = new List<Tile>();
    private const float TweenDuration = 0.25f;

    private void Awake() => Instance = this;

    private void Start()
    {
        // Сгенерировать поле: будем повторять генерацию пока не найдём возможный ход
        GenerateUntilPlayable();
    }

    private void GenerateUntilPlayable()
    {
        // Защита от зацикливания — не должно происходить в нормальных условиях,
        // но ограничим число попыток, чтобы не морочить бесконечно.
        const int maxAttempts = 1000;
        int attempts = 0;
        do
        {
            GenerateBoard();
            attempts++;
            if (attempts >= maxAttempts)
            {
                Debug.LogWarning("Board: reached max generation attempts. Proceeding with current board.");
                break;
            }
        } while (!HasPossibleMoves());
    }

    private void GenerateBoard()
    {
        // Инициализируем массив Tiles исходя из rows
        Tiles = new Tile[rows.Max(row => row.tiles.Length), rows.Length];

        for (var y = 0; y < rows.Length; y++)
        {
            for (var x = 0; x < rows[y].tiles.Length; x++)
            {
                var tile = rows[y].tiles[x];

                tile.x = x;
                tile.y = y;

                // Прямо устанавливаем случайный Item через свойство (обновится и иконка)
                tile.Item = ItemDatabase.Items[Random.Range(0, ItemDatabase.Items.Length)];

                Tiles[x, y] = tile;

                // Убедимся, что кнопка вызывает текущий Board.Select (у тебя Instance существует),
                // но если тебе нужно избегать Instance — можно привязывать делегат здесь к this.
                tile.button.onClick.RemoveAllListeners();
                tile.button.onClick.AddListener(() => Select(tile));
            }
        }

        // Убрать начальные готовые совпадения — делаем несколько проходов, пока есть тройки
        RemoveInitialMatches();
    }

    private void RemoveInitialMatches()
    {
        // Повторяем несколько раз, пока есть стартовые совпадения — чтобы полностью избавиться от них
        bool hadMatches;
        int guard = 0;
        do
        {
            hadMatches = false;
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var current = Tiles[x, y];

                    // Горизонтальная тройка справа-налево
                    if (x >= 2)
                    {
                        if (Tiles[x - 1, y].Item == current.Item && Tiles[x - 2, y].Item == current.Item)
                        {
                            current.Item = GetRandomItemExcept(current.Item);
                            hadMatches = true;
                        }
                    }

                    // Вертикальная тройка сверху-вниз
                    if (y >= 2)
                    {
                        if (Tiles[x, y - 1].Item == current.Item && Tiles[x, y - 2].Item == current.Item)
                        {
                            current.Item = GetRandomItemExcept(current.Item);
                            hadMatches = true;
                        }
                    }
                }
            }
            guard++;
            // Защитимся от бесконечного цикла (в случае маленького набора Item'ов)
            if (guard > 10) break;
        } while (hadMatches);
    }

    private Item GetRandomItemExcept(Item excluded)
    {
        // Если в базе только один элемент — вернём его (защита)
        if (ItemDatabase.Items.Length <= 1) return ItemDatabase.Items[0];

        Item newItem;
        do
        {
            newItem = ItemDatabase.Items[Random.Range(0, ItemDatabase.Items.Length)];
        } while (newItem == excluded);
        return newItem;
    }

    private bool HasPossibleMoves()
    {
        // Проверяем каждый тайл и его правого/нижнего соседа на возможность формирования совпадения
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var current = Tiles[x, y];

                if (x < Width - 1 && WouldFormMatchSimulated(x, y, x + 1, y)) return true;
                if (y < Height - 1 && WouldFormMatchSimulated(x, y, x, y + 1)) return true;
            }
        }
        return false;
    }

    // Симулируем обмен двух тайлов (x1,y1) и (x2,y2) без изменения реальных Tile.Item
    // и проверяем, приведёт ли он к появлению совпадения (тройки или больше).
    private bool WouldFormMatchSimulated(int x1, int y1, int x2, int y2)
    {
        var aItem = Tiles[x1, y1].Item;
        var bItem = Tiles[x2, y2].Item;

        // Если предметы одинаковые — обмен ничего не изменит
        if (aItem == bItem) return false;

        // Проверим вокруг первой позиции, если на ней окажется bItem
        if (CountConnectedSimulated(x1, y1, bItem, x2, y2) >= 3) return true;

        // И вокруг второй позиции, если на ней окажется aItem
        if (CountConnectedSimulated(x2, y2, aItem, x1, y1) >= 3) return true;

        return false;
    }

    // Возвращает размер соединённой области (flood fill) при условии,
    // что в позиции (sx,sy) рассматриваемый Item = candidate.
    // swapX/swapY указывают позицию второго меняемого тайла (чтобы учитывать симуляцию)
    private int CountConnectedSimulated(int sx, int sy, Item candidate, int swapX, int swapY)
    {
        var visited = new bool[Width, Height];
        var stack = new Stack<(int x, int y)>();
        stack.Push((sx, sy));
        int count = 0;

        while (stack.Count > 0)
        {
            var (cx, cy) = stack.Pop();

            if (cx < 0 || cy < 0 || cx >= Width || cy >= Height) continue;
            if (visited[cx, cy]) continue;

            // Определяем, какой Item у тайла (симуляция: если это swapX/swapY, используем "поменянный" item)
            Item tileItem;
            if (cx == sx && cy == sy)
            {
                tileItem = candidate; // стартовая позиция — используем candidate
            }
            else if (cx == swapX && cy == swapY)
            {
                // На позиции swap находится предмет-поменянный: если мы проверяем вокруг sx,swap позиция должна иметь другой предмет
                // Здесь candidate для swap не передается напрямую — но вызов CountConnectedSimulated всегда передаёт
                // swapX/swapY как позицию второго тайла. При проверке вокруг swap мы вызываем функцию с другими аргументами.
                tileItem = Tiles[cx, cy].Item; // оставляем текущий - мы учитываем swap только для sx/second check
            }
            else
            {
                tileItem = Tiles[cx, cy].Item;
            }

            // Когда мы попали на стартовую клетку sx, sy — tileItem == candidate (сделано выше)
            if (tileItem != candidate) continue;

            visited[cx, cy] = true;
            count++;

            // Добавляем соседей
            // left
            if (cx - 1 >= 0 && !visited[cx - 1, cy]) stack.Push((cx - 1, cy));
            // right
            if (cx + 1 < Width && !visited[cx + 1, cy]) stack.Push((cx + 1, cy));
            // up
            if (cy - 1 >= 0 && !visited[cx, cy - 1]) stack.Push((cx, cy - 1));
            // down
            if (cy + 1 < Height && !visited[cx, cy + 1]) stack.Push((cx, cy + 1));
        }

        return count;
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.A)) return;

        foreach (var connectedTile in Tiles[0, 0].GetConnectedTiles()) 
            connectedTile.icon.transform.DOScale(1.25f, TweenDuration).Play();
    }

    public async void Select(Tile tile)
    {
        if (!_selection.Contains(tile))
        {
            if (_selection.Count > 0)
            {
                if (Array.IndexOf(_selection[0].Neighbours, tile) != -1) _selection.Add(tile);
            }
            else
            {
                _selection.Add(tile);
            }
        }

        if (_selection.Count < 2) return;

        Debug.Log($"Selected tiles at ({_selection[0].x}, {_selection[0].y}) and ({_selection[1].x}, {_selection[1].y})");

        await Swap(_selection[0], _selection[1]);

        if (CanPop())
        {
            Pop();
        }
        else
        {
            await Swap(_selection[0], _selection[1]);
        }

        _selection.Clear();
    }

    public async Task Swap(Tile tile1, Tile tile2)
    {
        var icon1 = tile1.icon;
        var icon2 = tile2.icon;
        
        var icon1Transform = icon1.transform;
        var icon2Transform = icon2.transform;
        
        var sequence = DOTween.Sequence();

        sequence.Join(icon1Transform.DOMove(icon2Transform.position, TweenDuration))
                .Join(icon2Transform.DOMove(icon1Transform.position, TweenDuration));
        
        await sequence.Play().AsyncWaitForCompletion();

        icon1Transform.SetParent(tile2.transform);
        icon2Transform.SetParent(tile1.transform);
        
        tile1.icon = icon2;
        tile2.icon = icon1;

        var tile1Item = tile1.Item;
        
        tile1.Item = tile2.Item;
        tile2.Item = tile1Item;
    }

    private bool CanPop()
    {
        for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
                if (Tiles[x, y].GetConnectedTiles().Skip(1).Count() >= 2) 
                    return true;
            
        return false;
    }

    private async void Pop()
    {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var tile = Tiles[x, y];
                
                var connectedTiles = tile.GetConnectedTiles();

                if (connectedTiles.Skip(1).Count() < 2) continue;
                
                var deflateSequence = DOTween.Sequence();

                foreach (var connectedTile in connectedTiles) deflateSequence.Join(connectedTile.icon.transform.DOScale(Vector3.zero, TweenDuration));

                audioSource.PlayOneShot(collectSound);
                
                ScoreCounter.Instance.Score += tile.Item.value * connectedTiles.Count;
                
                await deflateSequence.Play()
                    .AsyncWaitForCompletion();
                
                var inflateSequence = DOTween.Sequence();
                
                foreach (var connectedTile in connectedTiles)
                {
                    connectedTile.Item = ItemDatabase.Items[Random.Range(0, ItemDatabase.Items.Length)];
                    
                    inflateSequence.Join(connectedTile.icon.transform.DOScale(Vector3.one, TweenDuration));
                }
                
                await inflateSequence.Play()
                    .AsyncWaitForCompletion();

                x = 0;
                y = 0;
            }
        }
    }
}
