   M S B A M L   `   `   `  ����C  ?SnakeWPF, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:SnakeWPFSnakeWPF  5   	       SnakeWPF.MainWindow   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   (localclr-namespace:SnakeWPF     5   	   -   5          � Title$  
MainWindow��5   	   $	��600��6   $
��1000��6)   
���   � � WindowStartupLocation$ CenterScreen=�5
   	    � 
ResizeMode$ NoResize=�5   	   .��5      � -   5         �Name$ MainGrid��5      $��0,10,0,0q�6   .+�5   
   �� -   5         ��Name$ 
EnemyScore��5      $��22(�5      $��SCORE: 0��5       �� TextAlignment$ Center=�5      $
��Left=�5      $	��Top=�5      $��Arial)�5      $
��Bold"�5      
�������$��
712,27,0,0q�5      $��40��6(   $	��126��64   �� 5   
   -   5        $ PlayerScore��5      $��22(�5      $��SCORE: 0��5      $ Center=�5      $
��Left=�5      $	��Top=�5       $��Arial)�5!      $
��Bold"�5"      
�������$��
162,27,0,0q�5$      $��40��6(   $	��126��64   �� 5%   
   -   5        $ 
PlayerWins��5%      $��22(�5&      $��Win: 0��5'      $ Center=�5(      $
��Left=�5)      $	��Top=�5*      $��Arial)�5+      $
��Bold"�5,      
�������$��	25,27,0,0q�5.      $��40��6'   $	��126��63   �� 5/   
   -   5        $ 	EnemyWins��5/      $��22(�50      $��Win: 0��51      $ Center=�52      $
��Left=�53      $	��Top=�54      $��Arial)�55      $
��Bold"�56      
�������$��
575,27,0,0q�58      $��40��6(   $	��126��64   �� 5:   
   -   5         ��Name$ GridBorder1��5:      
�������$��3q�5<      $
��Left=�5=      $��Center=�5>      $��50,0,0,0q�5?      ._�5@      A� -   5         A�Name$ 
playerGrid��5@      ���#1B0051$	��400��5B      $	��400��5C      $��0,0,0,0q�5D      $
��Left=�5E      $��Center=�5F       J� SnapsToDevicePixels$
 True��5G      	 �� BitmapScalingMode$	 HighQuality=�5H      5J      �� 5K   
   -	   5        $ GridBorder2��5K      
�������$��3q�5M      $��Right=�5N      $��Center=�5O      $��0,0,50,0q�5P      ._�5Q      A� -
   5        $ 	enemyGrid��5Q      ���#001F3F$	��400��5S      $	��400��5T      $��0,0,0,0q�5U      $��Right=�5V      $��Center=�5W      $
 True��5X      $	 HighQuality=�5Y      5Z      �� 5\   
   -   5        $ Overlay��5\      
�������
 �� EdgeMode$
 Aliased=�5^       J� RenderTransformOrigin$ 0.5,0.5&�5_       J� Opacity$	 0.8Q�5`      $��	0,-11,0,0q�5a      ._�5c      �� -   5        $ OverlayText��5c      $��PRESS ANY KEY TO START��5d      $��24(�5e      $��Center=�5f      $��Center=�5g      5i      5k      5n      