using MessagePack;

namespace VisionProcess.Persistence
{
    [MessagePackObject]
    public class MainViewDto
    {
        [Key(0)]
        public List<EditorDto> Editors { get; set; } = new List<EditorDto>();

        [Key(1)]
        public bool AutoSelectNewEditor { get; set; }

        [Key(2)]
        public Guid? SelectedEditorId { get; set; }
    }
}
