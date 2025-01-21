using System;
using System.Numerics;

namespace bubble_puzzle;

public static class GameConstants
{
    public const bool DEBUG_MODE = true;
    public const int TILE_SIZE = 64;
    public const int SCORE_TILE_SIZE = 32;
    public const int HITBOX_SIZE = 45;

    public const int BOARD_WIDTH = 8;
    public const int BOARD_HEIGHT = 13;

    public const int GAME_WINDOW_WIDTH = 15 * TILE_SIZE;
    public const int GAME_WINDOW_HEIGHT = 15 * TILE_SIZE;

    public const int GAME_OVER_LINE = 11;
    public static Vector2 BOARD_POSITION = new Vector2(TILE_SIZE, TILE_SIZE);
    public static Vector2 SCORE_POSITION = new Vector2(TILE_SIZE * 10, TILE_SIZE);
    public static Vector2 SHOOT_POSITION = new Vector2(((TILE_SIZE * 4) + (TILE_SIZE / 2)), TILE_SIZE * 11);
    public static Vector2 SHOOT_PIVOT_POSITION = new  Vector2(SHOOT_POSITION.X + (TILE_SIZE / 2), SHOOT_POSITION.Y + (TILE_SIZE / 2));

    public static Vector2 DEBUG_POSITION = new Vector2(TILE_SIZE * 10, TILE_SIZE * 3);
    public static float MOVE_SPEED = TILE_SIZE / 2;

    public static Random random = new Random();
}