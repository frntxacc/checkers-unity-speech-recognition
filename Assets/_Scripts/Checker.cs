﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Checker : MonoBehaviour
{
    [SerializeField] Sprite _whiteCheckerSprite;
    [SerializeField] Sprite _blackCheckerSprite;
    [SerializeField] Sprite _whiteQueenSprite;
    [SerializeField] Sprite _blackQueenSprite;
    [SerializeField] SpriteRenderer _spriteRenderer;

    CheckerBoard _board;
    CheckerCoords _coords;

    public bool IsWhite { get; private set; }
    public bool IsQueen { get; private set; }

    public void BecomeQueen() {
        IsQueen = true;
        _spriteRenderer.sprite = IsWhite ? _whiteQueenSprite : _blackQueenSprite;
    }

    public void SetCoords(CheckerCoords newCoords) {
        _coords = newCoords;
        // Reached end
        if (IsWhite ? newCoords.Y == 8 : newCoords.Y == 1 && !IsQueen)
            BecomeQueen();
        transform.position = _board.CheckerToWorldCoords(newCoords);

    }

    public void Init(CheckerBoard board, CheckerCoords checkerCoords, bool isWhite) {
        _board = board;
        _coords = checkerCoords;
        IsWhite = isWhite;

        _spriteRenderer.sprite = isWhite ? _whiteCheckerSprite : _blackCheckerSprite;
    }

    public List<CheckerMove> GetValidMoves(bool includeSimpleMoves) {
        return IsQueen ? GetQueenMoves(includeSimpleMoves) : GetCheckerMoves(includeSimpleMoves);
    }

    public bool SomeoneMustKill() {
        var myTeam = (IsWhite ? _board.WhiteCheckers : _board.BlackCheckers);

        return myTeam.Any(checker => 
                    checker.GetValidMoves(false).Any(move => move.isKill));
    }


    public bool IsValidMove(CheckerCoords newCoords) {

        if (_board.OutsideBoard(newCoords) || _board.isWhiteMove != IsWhite)
            return false;
        int yRemainder = newCoords.Y % 2, xRemainder = newCoords.X % 2;
        if (yRemainder != xRemainder)
            return false;

        var deltaY = newCoords.Y - _coords.Y;
        var deltaX = newCoords.X - _coords.X;
        var absoluteDeltaX = Mathf.Abs(deltaX);
        var absoluteDeltaY = Mathf.Abs(deltaY);

        if (IsQueen) {
            if (absoluteDeltaY == absoluteDeltaX) {

            }
        }
        else {
            if (absoluteDeltaY == 1 && absoluteDeltaX == 1)
                return IsSimpleMoveValid(newCoords);
            if (absoluteDeltaY == 2 && absoluteDeltaX == 2)
                return IsKillMoveValid(newCoords, deltaY, deltaX);
        }
        return false;
    }

    // TODO: Test with someone must kill.
    bool IsSimpleMoveValid(CheckerCoords newCoords) {
        return _board.CheckerAt(newCoords) == null && !SomeoneMustKill();
    }
    
    bool IsKillMoveValid(CheckerCoords newCoords, int deltaY, int deltaX) {

        if (_board.CheckerAt(newCoords) == null) {
            var killCoords = _coords + new CheckerCoords(deltaY / 2, deltaX / 2);

            var killChecker = _board.CheckerAt(killCoords);
            return killChecker != null && killChecker.IsWhite != IsWhite;
        }

        return false;
    }

    List<CheckerMove> GetCheckerMoves(bool includeSimpleMoves) {
        var moves = new List<CheckerMove>();

        var ySign = IsWhite ? 1 : -1;

        var leftDiagonalCell = new CheckerCoords(ySign, -1);
        var rightDiagonalCell = new CheckerCoords(ySign, 1);

        var oneForwardLeft = _coords + leftDiagonalCell;
        var twoForwardLeft = oneForwardLeft + leftDiagonalCell;

        var oneForwardRight = _coords + rightDiagonalCell;
        var twoForwardRight = oneForwardRight + rightDiagonalCell;

        var twoBackwardsLeft = _coords + new CheckerCoords(-2 * ySign, -2);
        var twoBackwardsRight = _coords + new CheckerCoords(-2 * ySign, 2);

        if (includeSimpleMoves && IsValidMove(oneForwardLeft))
            moves.Add(new CheckerMove { destinationCoords = oneForwardLeft, oldCoords = _coords });
        else if (IsValidMove(twoForwardLeft))
            moves.Add(new CheckerMove() {
                destinationCoords = twoForwardLeft,
                killCoords = oneForwardLeft,
                isKill = true,
                oldCoords = _coords
            });

        if (includeSimpleMoves && IsValidMove(oneForwardRight))
            moves.Add(new CheckerMove { destinationCoords = oneForwardRight, oldCoords = _coords });
        else if (IsValidMove(twoForwardRight))
            moves.Add(new CheckerMove {
                destinationCoords = twoForwardRight,
                killCoords = oneForwardRight,
                isKill = true,
                oldCoords = _coords
            });

        if (IsValidMove(twoBackwardsLeft))
            moves.Add(new CheckerMove {
                destinationCoords = twoBackwardsLeft,
                isKill = true,
                killCoords = _coords + new CheckerCoords(-ySign, -1),
                oldCoords = _coords
            });

        if (IsValidMove(twoBackwardsRight))
            moves.Add(new CheckerMove {
                destinationCoords = twoBackwardsRight,
                isKill = true,
                killCoords = _coords + new CheckerCoords(-ySign, 1),
                oldCoords = _coords
            });

        return moves;
    }

    public bool CheckQueenMove(List<CheckerMove> movesList, CheckerCoords newCoords, 
                                        ref bool enemyBehind, ref CheckerCoords enemyCoords,
                                        bool includeSimpleMoves) {

        if (_board.OutsideBoard(newCoords) || _board.isWhiteMove != IsWhite)
            return false;
        int yRemainder = newCoords.Y % 2, xRemainder = newCoords.X % 2;
        if (yRemainder != xRemainder)
            return false;

        var deltaY = newCoords.Y - _coords.Y;
        var deltaX = newCoords.X - _coords.X;
        var absoluteDeltaX = Mathf.Abs(deltaX);
        var absoluteDeltaY = Mathf.Abs(deltaY);

        if (absoluteDeltaY == absoluteDeltaX) {
            var checker = _board.CheckerAt(newCoords);
            if (checker == null) {
                if (enemyBehind) {
                    movesList.Add(new CheckerMove {
                        destinationCoords = newCoords,
                        isKill = true,
                        killCoords = enemyCoords,
                        oldCoords = _coords
                    });
                }
                else if (includeSimpleMoves && !SomeoneMustKill()) {
                    movesList.Add(new CheckerMove {
                        destinationCoords = newCoords,
                        oldCoords = _coords
                    });

                }

                return true;
            }
            // Friend.
            if (checker.IsWhite == IsWhite) {
                return false;
            }
            // Enemy.
            if (checker.IsWhite != IsWhite) {
                if (enemyBehind)
                    return false;

                enemyBehind = true;
                enemyCoords = newCoords;
                return true;
            }
        }

        return false;
    }

    List<CheckerMove> GetQueenMoves(bool includeSimpleMoves) {
        var moves = new List<CheckerMove>();

        var leftTop1 = new CheckerCoords(1, -1);
        var rightTop1 = new CheckerCoords(1, 1);
        var bottomRight1 = new CheckerCoords(-1, 1);
        var bottomLeft1 = new CheckerCoords(-1, -1);

        var currentCheckPosition = _coords;
        var enemyBehind = false;
        var enemyCoords = new CheckerCoords();

        while (true) {
            currentCheckPosition += leftTop1;
            var checkResult = CheckQueenMove(moves, currentCheckPosition, ref enemyBehind, 
                ref enemyCoords, includeSimpleMoves);
            if (checkResult == false) {
                break;
            }
        }

        // TODO:
        currentCheckPosition = _coords;
        enemyBehind = false;
        enemyCoords = new CheckerCoords();

        return moves;
    }

    public void Die() {
        gameObject.SetActive(false);
    }
}
