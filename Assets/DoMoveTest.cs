using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class DoMoveTest : MonoBehaviour
{
    // Start is called before the first frame update
   async void Start()
    {
// sequential
        await transform.DOMoveX(2, 1);
        await transform.DOMoveZ(5, 2);

// parallel with cancellation
        var ct = this.GetCancellationTokenOnDestroy();

        await UniTask.WhenAll(
            transform.DOMoveX(10, 3).WithCancellation(ct),
            transform.DOScale(10, 3).WithCancellation(ct));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
