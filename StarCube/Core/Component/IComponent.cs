using LiteDB;

namespace StarCube.Core.Component
{
    public interface IComponent<O>
        where O : class, IComponentHolder<O>
    {
        /// <summary>
        /// 组件的类型
        /// </summary>
        public ComponentType<O> Type => Variant.type;

        /// <summary>
        /// 组件的 variant
        /// </summary>
        public ComponentVariant<O> Variant { get; }


        /* ~ 组件 owner start ~ */
        public bool HasOwner { get; }
        public O Owner { get; }

        /// <summary>
        /// 当 component 被添加到某容器中后调用此方法
        /// </summary>
        /// <param name="newOwner"></param>
        public void OnAddToOwner(O newOwner);

        /// <summary>
        /// 当 component 从某容器中删除后调用此方法
        /// </summary>
        public void OnRemoveFromOwner();
        /* ~ 组件 owner start ~ */



        /* ~ 组件保存 start ~ */
        public bool Dirty { get; }

        /// <summary>
        /// 显示标记此组件需要被保存
        /// </summary>
        public void MarkDirty();

        /// <summary>
        /// 将组件的属性存储到 bson 中
        /// </summary>
        /// <param name="bson"></param>
        public void Serialize(BsonDocument bson);
        /* ~ 组件保存 start ~ */
    }
}
