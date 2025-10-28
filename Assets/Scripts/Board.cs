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
        Tiles = new Tile[rows.Max(row => row.tiles.Length), rows.Length];

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var tile = rows[y].tiles[x];

                tile.x = x;
                tile.y = y;

                RandomFillBoard(tile);
                
                Tiles[x, y] = tile;
            }
        }
    }

    private void RandomFillBoard(Tile tile)
    {
        tile.Item = ItemDatabase.Items[Random.Range(0, ItemDatabase.Items.Length)];
    }
    
    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.A)) return;

        foreach (var connectedTile in Tiles[0, 0].GetConnectedTiles()) connectedTile.icon.transform.DOScale(1.25f, TweenDuration).Play();
        
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
        
        await sequence.Play()
            .AsyncWaitForCompletion();

        icon1Transform.SetParent(tile2.transform);
        icon2Transform.SetParent(tile1.transform);
        
        tile1.icon = icon2;
        tile2.icon = icon1;

        SwapItems(tile1, tile2);
    }

    private bool HasPossibleMoves()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var tile = Tiles[x, y];
                
                // Проверяем соседей (вправо и вниз)
                // чтобы не дублировать проверки
                var right = tile.Right;
                var bottom = tile.Bottom;

                if (right != null)
                {
                    SwapItems(tile, right);
                    if (CanPop())
                    {
                        SwapItems(tile, right); // откат
                        return true;
                    }
                    SwapItems(tile, right);
                }

                if (bottom != null)
                {
                    SwapItems(tile, bottom);
                    if (CanPop())
                    {
                        SwapItems(tile, bottom); // откат
                        return true;
                    }
                    SwapItems(tile, bottom);
                }
            }
        }
        
        return false;
    }

    private void SwapItems(Tile tile1, Tile tile2)
    {
        (tile1.Item, tile2.Item) = (tile2.Item, tile1.Item);
    }

    private async void RegenerateBoard()
    {
        Debug.Log("No more moves! Regenerating board...");
    
        const float flashDuration = 0.09f;
    
        // Анимация случайного исчезновения
        var hideSequence =  DOTween.Sequence();
        var tilesList = Tiles.Cast<Tile>().ToList();
        
        // Перемешиваем порядок плиток
        for (int i = 0; i < tilesList.Count; i++)
        {
            int randomIndex = Random.Range(i, tilesList.Count);
            (tilesList[i], tilesList[randomIndex]) = (tilesList[randomIndex], tilesList[i]);
        }

        foreach (var tile in tilesList)
        {
            hideSequence.Join(tile.icon.transform.DOScale(Vector3.zero, flashDuration)
                    .SetDelay(Random.Range(0f, 0.1f)) // Случайная задержка);
            );
        }

        await hideSequence.Play().AsyncWaitForCompletion();
        
        // Перегенерация
        do
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    RandomFillBoard(Tiles[x, y]);
        }
        while (!CanPop() && !HasPossibleMoves());
    
        // Анимация случайного появления
        var showSequence = DOTween.Sequence();
    
        foreach (var tile in tilesList)
        {
            showSequence.Join(tile.icon.transform.DOScale(Vector3.one, flashDuration)
                    .SetEase(Ease.OutBack)
                    .SetDelay(Random.Range(0f, 0.15f)) // Случайная задержка
            );
        }
    
        await showSequence.Play().AsyncWaitForCompletion();
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
                
                int gainedScore = tile.Item.value * connectedTiles.Count;
                if (tile.Item.isEnergy)
                    ScoreCounter.Instance.AddToEnergy(gainedScore);
                else
                    ScoreCounter.Instance.AddToGear(gainedScore);
                
                
                
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
        
        // Проверяем после каждого "взрыва" доски
        if (!HasPossibleMoves())
        {
            RegenerateBoard();
        }
    }
}
