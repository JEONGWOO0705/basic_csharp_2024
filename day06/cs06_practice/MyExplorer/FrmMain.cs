using System.Diagnostics;

namespace MyExplorer
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }

        #region "이벤트 핸들러" 

        // 가장 기본, 가장 먼저 실행됨
        private void FrmMain_Load(object sender, EventArgs e)
        {
            TreeNode root = TrvFolder.Nodes.Add("내컴퓨터");

            string[] drives = Directory.GetLogicalDrives(); // 내컴퓨터 논리 드라이브 전부 가져옴
            foreach (var drive in drives)
            {
                TreeNode node = root.Nodes.Add(drive);
                node.Nodes.Add("..."); // 아직 미지정, 최초의 상태로 SET
            }

            // LsvFile.View = View.LargeIcon;
        }

        // 열기 버튼 클릭 이벤트 핸들러
        private void BtnOpen_Click(object sender, EventArgs e)
        {

        }

        // 트리노드 선택후 이벤트 핸들러
        private void TrvFolder_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // 폴더에서 노드 선택하면 리스트 뷰에 파일 표시
            TreeNode current = e.Node;
            if (e.Node == null) { return; }

            string path = current.FullPath.Replace("\\\\", "\\");
            TxtPath.Text = path.Substring(path.IndexOf("\\") + 1); // '내컴퓨터\' 제거

            try
            {
                LsvFile.Items.Clear(); // 다른 폴더 내에 있던 이전 파일 정보 삭제
                                       // 현재폴더의 하위폴더 정보 디스플레이 
                string[] directories = Directory.GetDirectories(TxtPath.Text);
                foreach (var directory in directories)
                {
                    DirectoryInfo info = new DirectoryInfo(directory);
                    // 리스트 뷰 컬럼 이름, 수정일자, 유형, 크기 순서로 리스트 뷰 아이템 생성
                    ListViewItem item = new ListViewItem(new string[] { info.Name, info.LastWriteTime.ToString(), "파일 폴더", string.Empty });
                    item.ImageIndex = 1; // 리스트 뷰의 폴더 이미지 인덱스
                    LsvFile.Items.Add(item);
                }

                // 파일 리스트 업
                string[] files = Directory.GetFiles(TxtPath.Text); // 현재폴더내의 파일 정보
                foreach (var file in files)
                {
                    FileInfo info = new FileInfo(file);
                    ListViewItem item = new ListViewItem(new string[] { info.Name, info.LastWriteTime.ToString(), info.Extension, info.Length.ToString() });
                    item.ImageIndex = GetImageIndex(info.Extension); // 확장자의 종류에 따라 이미지 번호가 변경
                    LsvFile.Items.Add(item);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }



        // 트리 확장 축소 아이콘 클릭해서 확장되기 직전 이벤트 핸들러
        private void TrvFolder_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode current = e.Node;
            // 폼이 로드된후 최초의 상태라면
            if (current.Nodes.Count == 1 && current.Nodes[0].Text.Equals("..."))
            {
                current.Nodes.Clear(); // ...을 삭제

                // FullPath, 내컴퓨터 \C:\에서 C:\만 남김
                String path = current.FullPath.Substring(current.FullPath.IndexOf("\\") + 1);

                try
                {
                    string[] directories = Directory.GetDirectories(path);
                    foreach (var directory in directories)
                    {
                        Debug.WriteLine(directory); // 디버그시만 출력
                        TreeNode newNode = current.Nodes.Add(directory.Substring(directory.LastIndexOf("\\") + 1));
                        newNode.ImageIndex = 1; // 미선택시 폴더 이미지
                        newNode.ImageIndex = 2; // 선택시 폴더 이미지
                        newNode.Nodes.Add("...");
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 리스트 뷰 마우스 클릭 이벤트 핸들러

        private void LsvFile_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // 컨텍스트 메뉴는 오른쪽 버튼에서만 동작
                CmsFiles.Show(LsvFile, e.Location); // 마우스 클릭한 위치에 보여준다
            }
        }

        private void TstMenuLargeIcon_Click(object sender, EventArgs e)
        {
            LsvFile.View = View.LargeIcon;
        }

        private void TstMenuSmallIcon_Click(object sender, EventArgs e)
        {
            LsvFile.View = View.SmallIcon;
        }


        private void TstMenuList_Click(object sender, EventArgs e)
        {
            LsvFile.View = View.List;
        }

        private void TstMenuDetails_Click(object sender, EventArgs e)
        {
            LsvFile.View = View.Details;
        }

        private void TstMenuTile_Click(object sender, EventArgs e)
        {
            LsvFile.View = View.Tile;
        }
        #endregion

        private int GetImageIndex(string extension)
        {
            // 3: 실행 파일, 4: 일반 파일, 5: txt파일
            var index = -1;
            switch (extension.ToLower())
            {
                case ".exe":
                    index = 3;
                    break;

                case ".txt":
                    index = 5;
                    break;

                default:
                    index = 4;
                    break;
            }
            return index;
        }

        // 리스트 뷰 아이템 더블 클릭 이벤트 핸들러, 실행파일 실행
        private void LsvFile_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                var extension = LsvFile.SelectedItems[0].Text.Split(".")[1];

                if (extension == "exe")
                {
                    //MessageBox.Show(LsvFile.SelectedItems[0].Text); // 디버깅용
                    // 실행파일의 경로는 TxtPath
                    var fullPath = TxtPath.Text + "\\" + LsvFile.SelectedItems[0].Text;
                    Process.Start(fullPath); // 외부 프로그램 실행
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"오류",MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
    }
}
