# Problems
## Definition
Problems 记录了 Tune 开发过程中遇到的未解决问题，包含问题TAG，是否解决（解决方案），触发位置，记录时间，重要性及其类别和解释。

## Content
### NAME_REPEAT_PROBLEM

#### 问题位置

|类|解决|
|---|---|
|Tune.Components.UI.CreatePlaylist|False|
|Tune.Components.UI.SelectPlaylist|False|

#### 问题描述
如果将结构改为：
```
<frame:WindowFrame Title="Select Playlist">
	<Grid>
		<!--此处放置控件-->
		<TextBlock x:Name="DescriptionTextBlock" Text="选择歌单来继续操作" FontSize="28"/>
		<TextBlock x:Name="DescriptionTextBlock1" Text="选择歌单来继续操作1" FontSize="28"/>
	</Grid>
</frame:WindowFrame>
```
就会触发以下编译错误
```
无法对元素“TextBlock”设置 Name 特性值“DescriptionTextBlock”。“TextBlock”在元素“WindowFrame”的范围内，在另一范围内定义它时，已注册了名称。 行 89 位置 36.
```
即使将`DescriptionTextBlock`改为其他内容，也会触发此编译错误
即使删除`<TextBlock x:Name="DescriptionTextBlock" Text="选择歌单来继续操作" FontSize="28"/>`，也会因下一个带有名称标签的元素而报错。

#### 解决方案
暂无

#### 记录
2026/8/22 一般重要 结构不规范

### SHOW_TOAST

#### 问题位置
散布在项目

#### 问题描述
当前仅为默认 MessageBox 提示，太突兀（有种electron感）。替换为其他显示方案。

#### 解决方案
暂无

#### 记录
2026/8/28 一般重要


### 草稿
```
<UserControl.Resources>
        <Style x:Key="MyTabItemStyle" TargetType="ListBoxItem">
            <!-- 字形相关：全程不变 -->
            <Setter Property="Cursor" Value="Hand"/>
            <Setter Property="Foreground" Value="#6B7280"/>
            <Setter Property="FontSize" Value="14"/>
            <Setter Property="Margin" Value="8,0,0,0"/>

            <!-- 固定宽度 -->
            <Setter Property="Width" Value="160"/>

            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="ListBoxItem">
                        <Grid SnapsToDevicePixels="True">
                            <!-- 未选中背景 -->
                            <Image x:Name="StandbyBg"
                           Stretch="Uniform"
                           Source="/Assets/Components/Tab/TabStandby.png"/>

                            <!-- 选中背景，默认隐藏 -->
                            <Image x:Name="ClickedBg"
                           Stretch="Uniform"
                           Visibility="Collapsed"
                           Source="/Assets/Components/Tab/TabClicked.png"/>

                            <!-- 文字：居中显示，字体不受任何 Trigger 影响 -->
                            <ContentPresenter HorizontalAlignment="Center"
                                      VerticalAlignment="Center"/>
                        </Grid>

                        <ControlTemplate.Triggers>
                            <Trigger Property="IsSelected" Value="True">
                                <Setter TargetName="StandbyBg" Property="Visibility" Value="Collapsed"/>
                                <Setter TargetName="ClickedBg" Property="Visibility" Value="Visible"/>
                            </Trigger>
                            <Trigger Property="IsEnabled" Value="False">
                                <Setter Property="Opacity" Value="0.5"/>
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>
    </UserControl.Resources>
```