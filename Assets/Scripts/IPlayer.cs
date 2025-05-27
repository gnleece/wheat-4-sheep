using Grid;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public interface IPlayer
{
    public int PlayerId { get; }

    public void Initialize(int playerId, IBoardManager boardManager);

    public Task PlaceFirstSettlementAndRoadAsync();

    public Task PlaceSecondSettlementAndRoadAsync();
}
