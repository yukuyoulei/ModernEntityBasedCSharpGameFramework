using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class EList<T> : Entity where T : Entity, new()
{
    private int count = -1;
    public string cellName;
    private string listName;
    private string cellPath;
    private Transform cellTemp;

    /// <summary>
    /// 使用CanvasGroup的alpha来显示或隐藏cell，需要在cell上挂载CanvasGroup和LayoutElement组件
    /// </summary>
    public bool useAlphaActive = false;

    public Action<int, T> onDataHandler;
    public Func<int, Transform, T> onInitHandler;
    public Func<int, Task<T>> onDynamicInitHandler;

    private List<T> cells = new List<T>();

    // 无限列表相关字段
    private bool isInfiniteMode = false;
    private ScrollRect scrollRect;
    private float cellSize = 100f;
    private bool isVertical = true;
    private int visibleCount = 0;
    private int bufferCount = 2;
    private int startIndex = 0;
    private RectTransform contentRect;

    private ToggleGroup toggleGroup;

    public ToggleGroup ToggleGroup
    {
        get
        {
            if (toggleGroup == null)
            {
                toggleGroup = transform.GetComponent<ToggleGroup>();
            }

            return toggleGroup;
        }
    }

    public static EList<T> CreateStatic(Entity parent, string refName)
    {
        return CreateStatic(parent, parent.GetMonoComponent<Transform>(refName), null, (Action<int, T>)null);
    }

    public static EList<T> CreateStatic(Entity parent, string refName, Action<int, T> onDataHandler)
    {
        return CreateStatic(parent, parent.GetMonoComponent<Transform>(refName), null, onDataHandler);
    }

    public static EList<T> CreateStatic(Entity parent, string refName, Action<int, T, EList<T>> onDataHandler)
    {
        return CreateStatic(parent, parent.GetMonoComponent<Transform>(refName), null, onDataHandler);
    }

    public static EList<T> CreateStatic(Entity parent, string refName, Func<int, Transform, T> onInitHandler, Action<int, T> onDataHandler)
    {
        return CreateStatic(parent, parent.GetMonoComponent<Transform>(refName), onInitHandler, onDataHandler);
    }

    public static EList<T> CreateStatic(Entity parent, string refName, Func<int, Transform, EList<T>, T> onInitHandler, Action<int, T, EList<T>> onDataHandler)
    {
        return CreateStatic(parent, parent.GetMonoComponent<Transform>(refName), onInitHandler, onDataHandler);
    }

    public static EList<T> CreateStatic(Entity parent, Transform refe, Action<int, T> onDataHandler)
    {
        return CreateStatic(parent, refe, null, onDataHandler);
    }
    public static EList<T> CreateStatic(Entity parent, Transform refe, Func<int, Transform, T> onInitHandler, Action<int, T> onDataHandler)
    {
        var clist = parent.AddChild<EList<T>>(refe);
        clist.Init(onInitHandler, onDataHandler);
        return clist;
    }

    public static EList<T> CreateStatic(Entity parent, Transform refe, Func<int, Transform, EList<T>, T> onInitHandler, Action<int, T, EList<T>> onDataHandler)
    {
        var clist = parent.AddChild<EList<T>>(refe);
        clist.Init(onInitHandler, onDataHandler);
        return clist;
    }

    //动态创建：只传父节点和list组件名
    public static EList<T> CreateDynamic(Entity parent, string listName, string cellPath)
    {
        return CreateDynamic(parent, listName, null, (Action<int, T>)null, cellPath);
    }

    //动态创建：只传父节点、list组件名和刷新方法名
    public static EList<T> CreateDynamic(Entity parent, string listName, Action<int, T> onDataHandler, string cellPath)
    {
        return CreateDynamic(parent, listName, null, onDataHandler, cellPath);
    }

    public static EList<T> CreateDynamic(Entity parent, string listName, Action<int, T, EList<T>> onDataHandler, string cellPath)
    {
        return CreateDynamic(parent, listName, null, onDataHandler, cellPath);
    }
    public static EList<T> CreateDynamic(Entity parent, string listName, Action<int, T> onDataHandler, Transform cellTemp)
    {
        return CreateDynamic(parent, listName, null, onDataHandler, cellTemp);
    }

    //动态创建：传父节点、list组件名、初始化方法名、刷新方法名
    public static EList<T> CreateDynamic(Entity parent, string listName, Func<int, Task<T>> onDynamicInitHandler, Action<int, T> onDataHandler, string cellPath)
    {
        Transform refe = parent.GetMonoComponent<Transform>(listName);

        var clist = parent.AddChild<EList<T>>(refe);
        clist.Init(onDynamicInitHandler, onDataHandler, listName, cellPath);
        return clist;
    }

    public static EList<T> CreateDynamic(Entity parent, string listName, Func<int, EList<T>, Task<T>> onDynamicInitHandler, Action<int, T, EList<T>> onDataHandler, string cellPath)
    {
        Transform refe = parent.GetMonoComponent<Transform>(listName);

        var clist = parent.AddChild<EList<T>>(refe);
        clist.Init(onDynamicInitHandler, onDataHandler, listName, cellPath);
        return clist;
    }
    public static EList<T> CreateDynamic(Entity parent, string listName, Func<int, EList<T>, Task<T>> onDynamicInitHandler, Action<int, T> onDataHandler, Transform cellTemp)
    {
        Transform refe = parent.GetMonoComponent<Transform>(listName);

        var clist = parent.AddChild<EList<T>>(refe);
        clist.Init(onDynamicInitHandler, onDataHandler, listName, cellTemp);
        return clist;
    }

    private void Init(Func<int, Transform, T> onInitHandler, Action<int, T> onDataHandler)
    {
        this.onInitHandler = onInitHandler;
        this.onDataHandler = onDataHandler;
    }

    private void Init(Func<int, Transform, EList<T>, T> onInitHandler, Action<int, T, EList<T>> onDataHandler)
    {
        Func<int, Transform, T> castOnInitHandler = onInitHandler == null ? null : (index, trans) => onInitHandler.Invoke(index, trans, this);
        Action<int, T> castOnDataHandler = onDataHandler == null ? null : (index, cell) => onDataHandler.Invoke(index, cell, this);
        Init(castOnInitHandler, castOnDataHandler);
    }

    private void Init(Func<int, Task<T>> onDynamicInitHandler, Action<int, T> onDataHandler, string listName, Transform cellTemp)
    {
        cellTemp.gameObject.SetActive(false);
        this.listName = listName;
        this.cellTemp = cellTemp;
        this.onDataHandler = onDataHandler;
        this.onDynamicInitHandler = onDynamicInitHandler;
    }
    private void Init(Func<int, Task<T>> onDynamicInitHandler, Action<int, T> onDataHandler, string listName, string cellPath)
    {
        this.listName = listName;
        this.cellPath = cellPath;
        this.onDataHandler = onDataHandler;
        this.onDynamicInitHandler = onDynamicInitHandler;
    }

    private void Init(Func<int, EList<T>, Task<T>> onDynamicInitHandler, Action<int, T, EList<T>> onDataHandler, string listName, string cellPath)
    {
        Func<int, Task<T>> castOnDynamicInitHandler = onDynamicInitHandler == null ? null : (index) => onDynamicInitHandler(index, this);
        Action<int, T> castOnDataHandler = onDataHandler == null ? null : (index, cell) => onDataHandler.Invoke(index, cell, this);
        Init(castOnDynamicInitHandler, castOnDataHandler, listName, cellPath);
    }
    private void Init(Func<int, EList<T>, Task<T>> onDynamicInitHandler, Action<int, T> onDataHandler, string listName, Transform cellTemp)
    {
        Func<int, Task<T>> castOnDynamicInitHandler = onDynamicInitHandler == null ? null : (index) => onDynamicInitHandler(index, this);
        Init(castOnDynamicInitHandler, onDataHandler, listName, cellTemp);
    }

    public void Static(int count = -1)
    {
        if (count < 0)
            count = transform.childCount;
        else
            count = Mathf.Min(count, transform.childCount);

        if (this.count == count)
        {
            Refresh();
            return;
        }

        this.count = count;
        for (int i = transform.childCount - 1; i >= count; i--)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        for (int i = 0; i < count; i++)
        {
            if (i >= cells.Count)
            {
                var tr = transform.GetChild(i);
                var cell = onInitHandler == null ? AddChild<T>(tr) : onInitHandler.Invoke(i, tr);
                if (cell != null)
                {
                    cells.Add(cell);
                }
            }

            cells[i].index = i;
            SetCellActive(cells[i], true);
            onDataHandler?.Invoke(i, cells[i]);
        }
    }

    public void StaticReset(int count)
    {
        for (int i = 0; i < cells.Count; i++)
        {
            SetCellActive(cells[i], i < count);
        }
    }

    public void Refresh()
    {
        if (onDataHandler == null)
            return;

        for (int i = 0; i < count; i++)
        {
            onDataHandler.Invoke(i, cells[i]);
        }
    }

    public async Task<bool> Dynamic(int count)
    {
        this.count = 0;
        for (int i = 0; i < count; i++)
        {
            if (i >= cells.Count)
            {
                T cell = null;
                if (onDynamicInitHandler == null)
                {
                    if (cellTemp == null)
                        cell = await DynamicAddByPath<T>(cellPath);
                    else
                    {
                        var tr = GameObject.Instantiate(cellTemp, cellTemp.transform.parent);
                        tr.gameObject.SetActive(true);
                        cell = AddChild<T>(tr.gameObject, true);
                    }
                }
                else
                    cell = await onDynamicInitHandler.Invoke(i);
                if (cell == null)
                    return true;

                if (!isActive)
                    return true;

                cell.transform.SetAsLastSibling();
                cell.transform.localScale = Vector3.one;
                cells.Add(cell);
            }

            cells[i].index = i;
            SetCellActive(cells[i], true);
            onDataHandler?.Invoke(i, cells[i]);
        }

        if (count < cells.Count)
        {
            for (int i = cells.Count - 1; i >= count; i--) //倒叙可以正常删除列表中的cell
            {
                SetCellActive(cells[i], false);
            }
        }

        this.count = count;
        return false;
    }

    public async Task<A> DynamicAddByPath<A>(string cellPath) where A : T, new()
    {
        return await AddChild<A>(cellPath);
    }

    public T Get(int i)
    {
        return i < count ? cells[i] : default(T);
    }

    public int Count()
    {
        return count;
    }

    /// <summary>
    /// 遍历所有的cell，包括不显示的
    /// </summary>
    public void ForeachAll(Action<T> action)
    {
        foreach (var cell in cells)
        {
            action.Invoke(cell);
        }
    }

    public void Clear()
    {
        for (int i = cells.Count - 1; i >= 0; i--) //倒叙可以正常删除列表中的cell
        {
            var cell = cells[i];
            cell?.OnDispose();
        }

        cells.Clear();
    }

    public void SetCellActive(T cell, bool active)
    {
        if (cell == null || cell.gameObject == null)
            return;
        if (useAlphaActive)
        {
            if (!cell.gameObject.activeSelf)
                cell.gameObject.SetActive(true);
            cell.SetAlphaActive(active, true);
        }
        else
        {
            cell.gameObject.SetActive(active);
        }
    }

    public IEnumerator<T> GetEnumerator() //返回迭代器
    {
        return cells.GetEnumerator();
    }

    #region 无限列表
    /*
     * ==================== 无限列表预设制作要求 ====================
     *
     * 1. ScrollRect 结构要求:
     *    ScrollView (ScrollRect组件)
     *    ├── Viewport (Mask组件)
     *    │   └── Content (EList挂载的节点)
     *
     * 2. Content 节点设置:
     *    - 必须有 RectTransform 组件
     *    - 垂直滚动: 锚点设为顶部(Top), pivot(0.5, 1)
     *    - 水平滚动: 锚点设为左侧(Left), pivot(0, 0.5)
     *    - 不要使用 VerticalLayoutGroup/HorizontalLayoutGroup
     *
     * 3. Cell 预设要求:
     *    - 必须有 RectTransform 组件
     *    - 垂直滚动: 锚点设为顶部(Top), pivot(0.5, 1)
     *    - 水平滚动: 锚点设为左侧(Left), pivot(0, 0.5)
     *    - 所有cell尺寸必须固定且相同，与cellSize参数一致
     *    - 不要使用 ContentSizeFitter
     *
     * 4. 注意事项:
     *    - 无限列表不支持不等高cell
     *    - 如需不等高，请使用 Dynamic() 方法
     *
     * ============================================================
      
        // 方式1：使用预制体路径
        var list1 = EList<MyCell>.CreateInfinite(parent, "Content", OnCellData, "Prefabs/Cell", 100f, true);

        // 方式2：使用模版Transform（新增）
        Transform cellTemplate = parent.GetMonoComponent<Transform>("CellTemplate");
        var list2 = EList<MyCell>.CreateInfinite(parent, "Content", OnCellData, cellTemplate, 100f, true);

        await list2.Infinite(1000);

     * ============================================================
     */

    /// <summary>
    /// 创建无限列表
    /// </summary>
    public static EList<T> CreateInfinite(Entity parent, string listName, Action<int, T> onDataHandler, string cellPath, float cellSize, bool isVertical = true)
    {
        Transform refe = parent.GetMonoComponent<Transform>(listName);
        var clist = parent.AddChild<EList<T>>(refe);
        clist.InitInfinite(onDataHandler, listName, cellPath, null, cellSize, isVertical);
        return clist;
    }

    /// <summary>
    /// 创建无限列表（使用模版Transform）
    /// </summary>
    public static EList<T> CreateInfinite(Entity parent, string listName, Action<int, T> onDataHandler, Transform cellTemp, float cellSize, bool isVertical = true)
    {
        Transform refe = parent.GetMonoComponent<Transform>(listName);
        var clist = parent.AddChild<EList<T>>(refe);
        clist.InitInfinite(onDataHandler, listName, null, cellTemp, cellSize, isVertical);
        return clist;
    }

    private void InitInfinite(Action<int, T> onDataHandler, string listName, string cellPath, Transform cellTemp, float cellSize, bool isVertical)
    {
        this.isInfiniteMode = true;
        this.listName = listName;
        this.cellPath = cellPath;
        this.cellTemp = cellTemp;
        this.onDataHandler = onDataHandler;
        this.cellSize = cellSize;
        this.isVertical = isVertical;

        if (cellTemp != null)
            cellTemp.gameObject.SetActive(false);

        scrollRect = transform.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            contentRect = transform as RectTransform;
            scrollRect.onValueChanged.AddListener(OnScrollChanged);
        }
    }

    /// <summary>
    /// 设置无限列表数据
    /// </summary>
    public async Task Infinite(int count)
    {
        if (!isInfiniteMode || scrollRect == null)
        {
            return;
        }

        this.count = count;
        startIndex = 0;

        // 计算可见区域能显示的cell数量
        var viewportSize = isVertical ? scrollRect.viewport.rect.height : scrollRect.viewport.rect.width;
        visibleCount = Mathf.CeilToInt(viewportSize / cellSize) + bufferCount * 2;

        // 设置content大小
        var contentSize = count * cellSize;
        if (isVertical)
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, contentSize);
        else
            contentRect.sizeDelta = new Vector2(contentSize, contentRect.sizeDelta.y);

        // 创建可见数量的cell
        int cellCount = Mathf.Min(visibleCount, count);
        for (int i = cells.Count; i < cellCount; i++)
        {
            T cell = null;
            if (cellTemp != null)
            {
                var tr = GameObject.Instantiate(cellTemp, contentRect);
                tr.gameObject.SetActive(true);
                cell = AddChild<T>(tr.gameObject, true);
            }
            else
            {
                cell = await DynamicAddByPath<T>(cellPath);
            }
            if (cell == null)
            {
                return;
            }

            cells.Add(cell);
        }

        // 隐藏多余的cell
        for (int i = cellCount; i < cells.Count; i++)
        {
            SetCellActive(cells[i], false);
        }

        UpdateInfiniteList(true);
    }

    private void OnScrollChanged(Vector2 pos)
    {
        if (!isInfiniteMode || count <= 0)
        {
            return;
        }

        UpdateInfiniteList(false);
    }

    private void UpdateInfiniteList(bool forceUpdate)
    {
        var scrollPos = isVertical ? contentRect.anchoredPosition.y : -contentRect.anchoredPosition.x;
        int newStartIndex = Mathf.Max(0, Mathf.FloorToInt(scrollPos / cellSize) - bufferCount);
        newStartIndex = Mathf.Min(newStartIndex, Mathf.Max(0, count - visibleCount));

        if (!forceUpdate && newStartIndex == startIndex)
        {
            return;
        }

        startIndex = newStartIndex;

        int cellCount = Mathf.Min(visibleCount, count);
        for (int i = 0; i < cellCount; i++)
        {
            int dataIndex = startIndex + i;
            if (dataIndex >= count)
            {
                SetCellActive(cells[i], false);
                continue;
            }

            var cell = cells[i];
            cell.index = dataIndex;
            SetCellActive(cell, true);

            // 设置cell位置
            var rt = cell.transform as RectTransform;
            if (rt != null)
            {
                if (isVertical)
                    rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -dataIndex * cellSize);
                else
                    rt.anchoredPosition = new Vector2(dataIndex * cellSize, rt.anchoredPosition.y);
            }

            onDataHandler?.Invoke(dataIndex, cell);
        }
    }

    /// <summary>
    /// 刷新无限列表
    /// </summary>
    public void RefreshInfinite()
    {
        if (isInfiniteMode)
            UpdateInfiniteList(true);
        else
            Refresh();
    }

    /// <summary>
    /// 跳转到指定索引的cell
    /// </summary>
    public void ScrollTo(int index)
    {
        if (!isInfiniteMode || scrollRect == null || count <= 0)
        {
            return;
        }

        index = Mathf.Clamp(index, 0, count - 1);
        var targetPos = index * cellSize;
        if (isVertical)
            contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, targetPos);
        else
            contentRect.anchoredPosition = new Vector2(-targetPos, contentRect.anchoredPosition.y);
        UpdateInfiniteList(true);
    }

    #endregion
}
