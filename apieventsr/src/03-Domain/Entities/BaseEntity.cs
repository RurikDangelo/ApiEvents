namespace apieventsr.Domain.Entities
{
    public class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; private set; }
        public DateTime? UpdateDate { get; private set; }
        public DateTime? DeleteDate { get; private set; }

        public void Create()
        {
            CreateDate = DateTime.UtcNow;
        }

        public void Delete()
        {
            DeleteDate = DateTime.UtcNow;
        }

        public void Update()
        {
            UpdateDate = DateTime.UtcNow;
        }

    }
}
