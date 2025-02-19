using System;
using System.Numerics;

namespace boba_burst;

public static class GameConstants
{
    public const bool DEBUG_MODE = false;
    public const int TILE_SIZE = 64;
    public const int SCORE_TILE_SIZE = 32;
    public const int HITBOX_SIZE = 45;

    public const int BOARD_WIDTH = 8;
    public const int BOARD_HEIGHT = 13;

    public const int GAME_WINDOW_WIDTH = 15 * TILE_SIZE;
    public const int GAME_WINDOW_HEIGHT = 15 * TILE_SIZE;

    public const int GAME_OVER_LINE = 10;
    public static Vector2 BOARD_POSITION = new Vector2(TILE_SIZE, TILE_SIZE);
    public static Vector2 SCORE_POSITION = new Vector2(TILE_SIZE * 10, TILE_SIZE);
    public static Vector2 SCORE_BOARD_POSITION = new Vector2(TILE_SIZE * 9, 0);
    public static Vector2 SHOOT_POSITION = new Vector2(((TILE_SIZE * 4) + (TILE_SIZE / 2)), TILE_SIZE * 11);
    public static Vector2 SHOOT_PIVOT_POSITION = new  Vector2(SHOOT_POSITION.X + (TILE_SIZE / 2), SHOOT_POSITION.Y + (TILE_SIZE / 2));

    public static Vector2 DEBUG_POSITION = new Vector2(TILE_SIZE * 10, TILE_SIZE * 3);
    public static Vector2 PLAY_BUTTON_POSITION = new Vector2(TILE_SIZE * 10, TILE_SIZE * 9);
    public static float MOVE_SPEED = TILE_SIZE * 20;

    public const int SAME_TYPE_RANDOM_LIMIT = 3;
    public const int FREEZE_TIME_DURATION = 3;
    public const int CEILING_TIME_DURATION = 5;

    public static Random random = new Random();
}