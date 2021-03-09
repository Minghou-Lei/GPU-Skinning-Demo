#GPU-Skinning-Demo
一种利用GPU来实现角色动画效果，减少CPU蒙皮开销的技术实现
- - -

  当场景中有很多人物动画模型的时候，性能会产生大量开销，其中很大一部分来自于骨骼动画。GPU Skinning是将CPU中的蒙皮工作转移到GPU中进行，从而能够较大地提升多角色场景的运行效率，在大规模群体动画模拟如MMO、RTS等游戏中有较大的应用性。
可以发现如果要渲染的动画角色数量很大时主要会有以下两个巨大的开销：

- CPU在处理动画时的开销。
- 每个角色一个Draw Call造成的开销。

CPU的这两大开销限制了我们使用传统方式渲染大规模角色的可能性，主要瓶颈是角色动画的处理都集中在CPU端。瓶颈之二是CPU和GPU之间的Draw Call问题，如果利用批处理（包括Static Batching和Dynamic Batching）或是从Unity5.4之后引入的GPU Instancing就可以解决这个问题。但是，不幸的是这两种技术都不支持动画角色的SkinnedMeshRender。

那么解决方案就呼之欲出了，那就是将动画相关的内容从CPU转移到GPU，同时由于CPU不需要再处理动画的逻辑了，因此CPU不仅省去了这部分的开销而且SkinnedMeshRender也可以替换成一般的Mesh Render，我们就可以很开心的使用GPU Instancing来减少Draw Call了。

## 1.渲染骨骼动画到材质
