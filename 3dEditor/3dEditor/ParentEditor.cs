﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using EditorLib;
using RayTracerLib;
using System.Runtime.InteropServices;
using Mathematics;
using System.IO;
using System.Xml;

namespace _3dEditor
{
    public partial class ParentEditor : Form
    {
        class MyForm : Form
        {
            public void method()
            {
                MessageBox.Show(this, "blablablablabla");
            }
        }

        private const int WM_HSCROLL = 277;//0x114;
        private const int WM_VSCROLL = 0x115;

        private const int SB_HORZ = 0;
        private const int SB_VERT = 1;

        private const int SB_LINELEFT = 0;
        private const int SB_LINERIGHT = 1;
        private const int SB_PAGELEFT = 2;
        private const int SB_PAGERIGHT = 3;
        private const int SB_THUMBPOSITION = 4;
        private const int SB_THUMBTRACK = 5;
        private const int SB_LEFT = 6;
        private const int SB_RIGHT = 7;
        private const int SB_ENDSCROLL = 8;

        private const int SIF_TRACKPOS = 0x10;
        private const int SIF_RANGE = 0x1;
        private const int SIF_POS = 0x4;
        private const int SIF_PAGE = 0x2;
        private const int SIF_ALL = SIF_RANGE | SIF_PAGE | SIF_POS | SIF_TRACKPOS;

        public WndBoard _WndBoard { get; private set; }
        public WndScene _WndScene { get; private set; }
        public WndProperties _WndProperties { get; private set; }

        public RayTracing _rayTracer;

        const double _ratioSize = 1.0 / 2.4;    // pomer vysek oken> hlavni okno: okno sceny
        public ParentEditor()
        {
            InitializeComponent();
            
            //
            // Okno Kreslici plochy
            //
            _WndBoard = new WndBoard();
            _WndBoard.MdiParent = this;
            _WndBoard.Show();
            _WndBoard.Invalidate();

            //
            // Okno sceny
            //
            _WndScene = new WndScene();
            _WndScene.MdiParent = this;
            _WndScene.Location = new Point(_WndBoard.Width, _WndBoard.Location.Y);
            _WndScene.Height = (int)(_WndBoard.Height * _ratioSize);
            int scrollWid = 2 * System.Windows.Forms.SystemInformation.VerticalScrollBarWidth;
            if (!this.VerticalScroll.Visible) 
                scrollWid = 10;
            _WndScene.Width = this.Width - _WndBoard.Width - scrollWid; 
            
            _WndScene.Show();
            _WndScene.Update();
            _WndScene.Activate();
            _WndScene.Invalidate();

            //
            // Okno s nastavenim
            //
            _WndProperties = new WndProperties();
            _WndProperties.MdiParent = this;
            _WndProperties.Location = new Point(_WndBoard.Width, _WndScene.Height);
            //_WndProperties.Width = _WndScene.Width;
            //_WndProperties.Height = (int)(_WndBoard.Height * (1 - _ratioSize));
            _WndProperties.SetPanelsSize(_WndScene.GetSize().Width, _WndScene.GetSize().Height);
            _WndProperties.Show();
            _WndProperties.Invalidate();
            //_WndProperties.Anchor = AnchorStyles.Right;
            //_WndScene.Paint += new PaintEventHandler

            Form[] fs = this.MdiChildren;
            //foreach (Form f in this.MdiChildren)
            //{
            //    f.ClientSizeChanged += new EventHandler(onClientSizeChange);
            //}

            //VScroll = true;
            //VScrollBar vs = new VScrollBar();
            //vs.Parent = this;
            //vs.Scroll += new ScrollEventHandler(this.onScroll);
            //vs.Dock = DockStyle.Right;
            //Controls.Add(vs);
            
            InitRayTracer();
            //LayoutMdi(MdiLayout.Cascade);

            //// TESTING
            //bool test1 = RTree.Testing1();
            //bool test2 = RTree.Testing2();
            //Cuboid.TestCuboidRayFromInside();
        }

        protected override void WndProc(ref Message m)
        {

            int i2 = 2;
            //ScrollableControl scc = new ScrollableControl();
            
            // Add event handlers for the OnScroll and OnValueChanged events.
            
            //vs.ValueChanged += new EventHandler(this.vScrollBar1_ValueChanged); 

            //VScrollProperties vsp = new VScrollProperties(ScrVScroll);
            switch (m.Msg)
            {
                case WM_HSCROLL:
                    int i = 2;
                    //ScrollInfoStruct si = new ScrollInfoStruct();
                    //si.fMask = SIF_ALL;
                    //si.cbSize = (uint)Marshal.SizeOf(si);
                    //GetScrollInfo(msg.HWnd, SB_VERT, ref si);
                    //if (msg.WParam.ToInt32() == SB_ENDSCROLL)
                    //{
                    //    ScrollEventArgs sargs = new ScrollEventArgs(ScrollEventType.EndScroll, si.nPos);
                    //    onScroll(this, sargs);
                    //}
                    break;
            }
            base.WndProc(ref m);
        }

        private void InitRayTracer()
        {
            _rayTracer = new RayTracing();
            Sphere sph1 = new Sphere(new Vektor(0, 0, 0), 1);// new Colour(1, 0.5, 0.1, 1));
            Sphere sph2 = new Sphere(new Vektor(-2, -1, -10), 1.5);
            Cube cube1 = new Cube(new Vektor(-2, 1, -3), new Vektor(1, 1, 1), 1);
            //Cube cube2 = new Cube(new Vektor(1.6, -0.1, -5.2), new Vektor(1, 1, 1), 1);
            Cube cube2 = new Cube(new Vektor(0, 0, 0), new Vektor(1, 1, 1), 1);
            cube2.Material.Color = Colour.ColourCreate(Color.Gold);
            Plane plane1 = new Plane(new Vektor(1, 1, 0), 3);
            Cylinder cyl = new Cylinder(new Vektor(3, 2, 1), new Vektor(1, 1, 1), 1, 8);

            Triangle tr1 = new Triangle(new Vektor(-1, -2, 0), new Vektor(0, -1 , 3), new Vektor(0, 1, 3));

            Cone cone1 = new Cone();
            Cone cone2 = new Cone(new Vektor(-3, 0, -3), new Vektor(1, 0, 0), 0.6, 3);
            Cone cone3 = new Cone(new Vektor(3, 0, -3), new Vektor(-1, 0, 0), 0.6, 3);
            Cone cone4 = new Cone(new Vektor(0, 3, -3), new Vektor(0, -1, 0), 0.8, 3);
            Cone cone5 = new Cone(new Vektor(0, -3, -3), new Vektor(0, 1, 0), 0.8, 3);

            _rayTracer.RScene.SceneObjects.Clear();

            _rayTracer.RScene.Lights[0].Coord = new Vektor(4.2, 2.1, 0.6);
            _rayTracer.RScene.Lights[1].Coord = new Vektor(0, 4, 5);
            _rayTracer.RCamera.Source = new Vektor(0, 0, 5);
            _rayTracer.RCamera.SetNormAndUp(new Vektor(0, 0, -1), new Vektor(0, 1, 0));
            //_rayTracer.RScene.SceneObjects.Add(sph1);
            _rayTracer.RScene.SceneObjects.Add(sph1);
            //_rayTracer.RScene.SceneObjects.Add(cube1);
            //_rayTracer.RScene.SceneObjects.Add(cube2);
            //_rayTracer.RScene.SceneObjects.Add(tr1);
            //_rayTracer.RScene.SceneObjects.Add(cone2);
            //_rayTracer.RScene.SceneObjects.Add(cone3);
            //_rayTracer.RScene.SceneObjects.Add(cone4);
            //_rayTracer.RScene.SceneObjects.Add(cone5);
            //_rayTracer.RScene.SceneObjects.Add(plane1);
            //_rayTracer.RScene.SceneObjects.Add(cyl);
            //sph2.IsActive = false;

            //_rayTracer.RScene.SetDefaultScene4();

            

            CustomObject custom = CustomObject.CreateCube();
            //_rayTracer.RScene.SceneObjects.Add(custom);

            CustomObject planeCustom = CustomObject.CreatePlane();
            //_rayTracer.RScene.SceneObjects.Add(planeCustom);

            //_rayTracer.RScene.SetDefaultSceneSphericCube();
            //_rayTracer.RScene.SetDefaultSceneAxes();
            //_rayTracer.RCamera = _rayTracer.RScene.Camera;
            this._WndBoard.AddRaytrScene(_rayTracer.RScene);

            RayImage img = new RayImage(1, new Colour(0.8, 0.1, 0.5, 0), false);
            img.BackgroundColor = _rayTracer.RScene.BgColor;
            this._WndScene.AddItem(img);
            _WndScene.ShowNode(img);

            Animation anim = new Animation();
            _WndBoard.AddAnimation(anim);

            //Octree octree = new Octree(_rayTracer.RScene.SceneObjects);

            

        }

        private void onMDIChildActivate(object sender, EventArgs e)
        {
        }

        private void onShown(object sender, EventArgs e)
        {
            this.MdiChildren[0].Activate();
            this.MdiChildren[1].Activate();
            this.MdiChildren[2].Activate();
        }

        private void onPaint(object sender, PaintEventArgs e)
        {
            //UpdateAll();
            ////_WndBoard.toolStrip1.Invalidate(true);
            //this.MdiChildren[0].Activate();//.Invalidate(true);
            //this.MdiChildren[1].Activate();//.Invalidate(true);
            //this.MdiChildren[2].Activate();// Invalidate(true);
        }

        private void onScroll(object sender, ScrollEventArgs e)
        {
            UpdateAll();
        }

        private void UpdateAll()
        {
            ////this.MdiChildren[0].Activate();
            //this.MdiChildren[0].Update();
            ////this.MdiChildren[1].Activate();
            //this.MdiChildren[1].Update();
            ////this.MdiChildren[2].Activate();
            //this.MdiChildren[2].Update();
            //this.MdiChildren[0].Activate();
            //this.MdiChildren[1].Activate();
            //this.MdiChildren[2].Activate();
            //this.MdiChildren[0].Focus();
            //this.MdiChildren[1].Focus();
            //this.MdiChildren[2].Focus();
            //this.MdiChildren[0].Invalidate(true);
            //this.MdiChildren[1].Invalidate(true);
            //this.MdiChildren[2].Invalidate(true);
            //this.Invalidate(true);
        }

        private void onScroll1(object sender, ScrollEventArgs e)
        {
            int i123 = 0;
        }

        private void onClick(object sender, EventArgs e)
        {
            int a = 0;
        }

        private void onMouse(object sender, MouseEventArgs e)
        {
            int a = 0;
        }

        private void onMDIActive(object sender, EventArgs e)
        {
            //this.MdiChildren[0].Invalidate(true);
            //this.MdiChildren[1].Invalidate(true);
            //this.MdiChildren[2].Invalidate(true);
            //this.Invalidate(true);
        }

        private void onDrawClick(object sender, EventArgs e)
        {
            _WndBoard.InitForRaytracer();
            RayImage img = new RayImage(_WndScene.GetSelectedImage());
            //_rayTracer.SetRayImage(img);
            RayTracing raytr = new RayTracing(_rayTracer);
            raytr.SetRayImage(img);
            DrawingBoard form = new DrawingBoard(this);
            DefaultShape.TotalTested = 0;
            form.Size = new Size(img.CurrentSize.Width + RayImage.SizeWidthExtent, img.CurrentSize.Height + RayImage.SizeHeightExtent);
            form.Set(raytr, img);
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            form.Show();
        }

        private void CheckAnimPath(Animation anim)
        {
            if (!Directory.Exists(Path.GetDirectoryName(anim.FileFullPath)))
            {
                anim.FileFullPath = _WndProperties.AnimFileSelect();
            }
            
        }
        private void onAnimeClick(object sender, EventArgs e)
        {
            _WndBoard.InitForRaytracer();

            RayImage rayImg = new RayImage(_WndScene.GetSelectedImage());

            RayTracing raytr = new RayTracing(_rayTracer);
            raytr.SetRayImage(rayImg);

            DrawingAnimation drAnim = _WndScene.GetSelectedAnimation();
            Animation anim = new Animation((Animation)drAnim.ModelObject);
            CheckAnimPath(anim);

            DefaultShape.TotalTested = 0;

            AnimBoard animForm = new AnimBoard(raytr, rayImg, anim, this);

            this.toolStripAnimate.Enabled = false;
            animForm.FormClosed += animForm_FormClosed;
            animForm.Show();
        }

        public void AddRaytrObject(object obj)
        {
            if (obj is DefaultShape)
                _rayTracer.RScene.AddSceneObject(obj as DefaultShape);
            else if (obj is Light)
                _rayTracer.RScene.AddLight(obj as Light);
            
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //String fullpath = Path.Combine(Application.StartupPath, "save.xml");
            ExternScene extsc = new ExternScene();
            RayImage[] images = _WndScene.GetImages();
            Animation[] anims = _WndScene.GetAnimations();
            _WndBoard.InitForRaytracer();
            double[] degs = _WndBoard.RotationMatrix.GetAnglesFromMatrix();
            extsc.Set(_rayTracer.RScene, images, anims, degs);

            try
            {
                this.saveFileDialog.InitialDirectory = Environment.CurrentDirectory;    // adresar instalace programu
            }
            catch (Exception)
            {
                try
                {
                    this.saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
                catch (Exception)
                {
                    this.saveFileDialog.InitialDirectory = "";
                }
            }
            _WndBoard.AllowPaint(false);
            if (this.saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string ext = Path.GetExtension(saveFileDialog.FileName);
                try
                {
                    ExternScene.SerializeXML(saveFileDialog.FileName, extsc);
                }
                catch (Exception)
                {
                    _WndBoard.AllowPaint(false);
                    MessageBox.Show("During saving were some problems. It is possible, that scene was not correctly saved.", 
                        "Error saving scene", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _WndBoard.AllowPaint(true);
                }
            }
            _WndBoard.AllowPaint(true);
        }

        void method()
        {
            
            MyForm f = new MyForm();
            f.TopMost = true;
            //_WndBoard.WindowState = FormWindowState.Minimized;
            //_WndBoard.TopMost = true;
            //_WndBoard.Visible = false;
            _WndBoard.AllowPaint(false);
            MessageBox.Show(this, "blablablablabla1");
            _WndBoard.AllowPaint(true);
        }
        /// <summary>
        /// nahrati nove sceny
        /// </summary>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _WndBoard.AllowPaint(false);
            RayTracing origRaytr = _rayTracer;
            RayImage[] origImgs = _WndScene.GetImages();
            Animation[] origAnims = _WndScene.GetAnimations();
            
            if (this.openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                RayImage[] imgs;
                Animation[] anims;
                Scene scene;
                ExternScene extsc;
                // nacteni sceny - jen deserializace
                try
                {
                    extsc = ExternScene.DeserializeXML(openFileDialog.FileName);
                    LabeledShape.ResetLabels();
                    scene = extsc.GetScene();
                    imgs = extsc.GetRayImgs();
                    anims = extsc.GetAnimations();
                }
                catch (Exception ex)
                {
                    _WndBoard.AllowPaint(false);
                    MessageBox.Show(ex.Message, "Error loading scene", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _WndBoard.AllowPaint(true);
                    return;
                }
                // zobrazeni nactene sceny
                try
                {
                    _WndScene.ClearAll();
                    
                    _rayTracer = new RayTracing(scene);
                    this._WndBoard.AddRaytrScene(_rayTracer.RScene);
                    _WndScene.AddImages(imgs);
                    _WndBoard.AddAnimations(anims);
                    //_WndScene.AddAnimations(anims);
                    RayImage imgsel = _WndScene.GetSelectedImage();
                    _WndScene.ShowNode(imgsel);
                    _WndBoard.SetRotationDegrees(extsc.EditorAngleX, extsc.EditorAngleY, extsc.EditorAngleZ);
                }
                catch (Exception ex)
                {
                    _WndBoard.AllowPaint(false);
                    MessageBox.Show(ex.Message,
                        "Error loading scene", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _WndBoard.AllowPaint(true);
                    _WndScene.ClearAll();
                    
                    _rayTracer = origRaytr;
                    this._WndBoard.AddRaytrScene(_rayTracer.RScene);
                    _WndScene.AddImages(origImgs);
                    _WndBoard.AddAnimations(anims);
                    RayImage imgsel = _WndScene.GetSelectedImage();
                    _WndScene.ShowNode(imgsel);
                }
            }
            _WndBoard.AllowPaint(true);
        }
        /// <summary>
        /// pridani objektu do sceny
        /// </summary>
        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _WndBoard.AllowPaint(false);
            RayTracing origRaytr = _rayTracer;
            RayImage[] origImgs = _WndScene.GetImages();
            Animation[] origAnims = _WndScene.GetAnimations();

            if (this.openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                RayImage[] imgs;
                Animation[] anims;
                Scene scene;
                ExternScene extsc;
                // nacteni sceny - jen deserializace
                try
                {
                    extsc = ExternScene.DeserializeXML(openFileDialog.FileName);
                    //LabeledShape.ResetLabels();
                    scene = extsc.GetScene();
                    imgs = extsc.GetRayImgs();
                    anims = extsc.GetAnimations();
                }
                catch (Exception ex)
                {
                    _WndBoard.AllowPaint(false);
                    MessageBox.Show(ex.Message, "Error loading scene", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _WndBoard.AllowPaint(true);
                    return;
                }
                // zobrazeni nactene sceny
                try
                {
                    
                    _rayTracer = new RayTracing(origRaytr.RScene);
                    AddSceneForm form = new AddSceneForm(scene, imgs, anims);
                    DialogResult result = form.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        scene = form.GetSelectedScene();
                        imgs = form.GetSelectedImages();
                        anims = form.GetSelectedAnimations();

                        _rayTracer.RScene = Scene.MergeScenes(_rayTracer.RScene, scene);
                        _rayTracer.RCamera = _rayTracer.RScene.Camera;
                        
                        //List<Animation> animList = new List<Animation>(origAnims);
                        //if (anims != null)
                        //    animList.AddRange(anims);
                        //anims = animList.ToArray();
                        anims = Animation.MergeAnimations(origAnims, anims);

                        //List<RayImage> imgList = new List<RayImage>(origImgs);
                        //if (imgs != null)
                        //    imgList.AddRange(imgs);
                        //imgs = imgList.ToArray();
                        imgs = RayImage.MergeRayImgs(origImgs, imgs);

                        _WndScene.ClearAll();
                        this._WndBoard.AddRaytrScene(_rayTracer.RScene);
                        _WndScene.AddImages(imgs);
                        _WndBoard.AddAnimations(anims);
                        RayImage imgsel = _WndScene.GetSelectedImage();
                        _WndScene.ShowNode(imgsel);
                        _WndBoard.SetRotationDegrees(extsc.EditorAngleX, extsc.EditorAngleY, extsc.EditorAngleZ);
                    }
                }
                catch (Exception ex)
                {
                    _WndBoard.AllowPaint(false);
                    MessageBox.Show(ex.Message,
                        "Error loading scene", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _WndBoard.AllowPaint(true);
                    _WndScene.ClearAll();

                    _rayTracer = origRaytr;
                    this._WndBoard.AddRaytrScene(_rayTracer.RScene);
                    _WndScene.AddImages(origImgs);
                    _WndBoard.AddAnimations(origAnims);
                    RayImage imgsel = _WndScene.GetSelectedImage();
                    _WndScene.ShowNode(imgsel);
                }
            }
            _WndBoard.AllowPaint(true);
        }

        /// <summary>
        /// Jedina spravna metoda na zobrazeni messageBoxu.
        /// Jedna se o seskupeni metod:
        ///     _WndBoard.AllowPaint(false);
        ///     MessageBox.Show(text, caption, msgButtons, msgIcon);
        ///     _WndBoard.AllowPaint(true);
        /// </summary>
        /// <param name="text">zprava msgboxu</param>
        /// <param name="caption">nadpis</param>
        /// <param name="msgButtons">tlacitka</param>
        /// <param name="msgIcon">icona</param>
        public DialogResult MessageBoxShow(string text, string caption, MessageBoxButtons msgButtons, MessageBoxIcon msgIcon)
        {
            _WndBoard.AllowPaint(false);
            DialogResult result= MessageBox.Show(text, caption, msgButtons, msgIcon);
            _WndBoard.AllowPaint(true);
            return result;
        }

        /// <summary>
        /// nastavi promenne pro zobrazeni message boxu, aby nebyl schovany za formularem
        /// musi se zavolat dvakrat - pred zobrazenim msgboxu a po nem
        /// </summary>
        /// <param name="isBeforeMsgBox">true pred msgBoxem, False po msgBoxu</param>
        public void PrepareForMsgBoxes(bool isBeforeMsgBox)
        {
            _WndBoard.AllowPaint(!isBeforeMsgBox);
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public bool _IsClosing;
        private void BeforeClose(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBoxShow("Are you sure you want to close this application?", "Confirm exit",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == System.Windows.Forms.DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        private void onClientSizeChange(object sender, EventArgs e)
        {
            //this.MdiChildren[0].Invalidate();
            //this.MdiChildren[1].Invalidate();
            //this.MdiChildren[2].Invalidate();
        }



        /// <summary>
        /// po zavreni formulare animace se opet povoli tlacitko animace
        /// </summary>
        /// <param name="sender">formular animace</param>
        /// <param name="e"></param>
        void animForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.toolStripAnimate.Enabled = true;
        }

        
        internal void SetAnimationEnabled(bool isEnabled)
        {
            this.toolStripAnimate.Enabled = isEnabled;
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Help help = new Help();
            help.ShowDialog();
        }

        
    }
}
